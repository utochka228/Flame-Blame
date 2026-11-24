using System;
using System.Collections;
using System.Collections.Generic;
using FlameBlame.Core.Logging;
using FlameBlame.Hose.Simulation;
using FlameBlame.Pressure.Components;
using FlameBlame.Pressure.Simulation;
using Microsoft.Extensions.Logging;
using UnityEngine;

namespace FlameBlame.Hose.Components
{
    public class HoseNetwork : MonoBehaviour
    {
        private const int SourcePartId = 0;

        private static readonly ILogger<HoseNetwork> Logger = Loggers.For<HoseNetwork>();

        [SerializeField] private PressureSource _source;
        [SerializeField] private HoseSegment _sourceHose;
        [SerializeField] private HoseSegment[] _hoses;
        [SerializeField] private HoseJoint[] _joints;
        [SerializeField] [Range(2, 64)] private int _maxPieces = 16;
        [SerializeField] [Min(0.1f)] private float _junctionBreakDistance = 1f;
        [SerializeField] [Range(1, 30)] private int _solvesPerSecond = 10;
        [SerializeField] [Range(1, 8)] private int _junctionIterations = 4;

        private readonly PressureGraph _graph = new PressureGraph();
        private readonly PressureResult _result = new PressureResult();
        private readonly List<float> _pinchPositions = new List<float>();

        private RopeWorld _world;
        private HosePinchSensor[] _sensors;
        private HoseBurstMonitor[] _monitors;
        private float[][] _profiles;
        private int[] _junctionIds;
        private int[] _splitterPartIds;
        private int _sourceHoseIndex;

        private void Start()
        {
            var pieceCount = _hoses.Length + _joints.Length;
            if (pieceCount > _maxPieces)
            {
                Logger.LogError("Hose network {Network} has {Count} pieces, over the cap of {Cap}", this, pieceCount, _maxPieces);
                enabled = false;
                return;
            }

            BuildWorld();
            StartCoroutine(SolveLoop());
        }

        private void FixedUpdate()
        {
            _world.Step(Time.fixedDeltaTime);
        }

        public RopeWorld World => _world;

        public Vector3 JointPosition(int jointIndex) => _world.JunctionPosition(_junctionIds[jointIndex]);

        private void BuildWorld()
        {
            _world = new RopeWorld(_hoses[0].Config.Substeps) { JunctionIterations = _junctionIterations };
            _sensors = new HosePinchSensor[_hoses.Length];
            _monitors = new HoseBurstMonitor[_hoses.Length];
            _profiles = new float[_hoses.Length][];
            for (var index = 0; index < _hoses.Length; index++)
            {
                _world.AddRope(_hoses[index].Rope);
                _hoses[index].IsSteppedByNetwork = true;
                _sensors[index] = _hoses[index].GetComponent<HosePinchSensor>();
                _monitors[index] = _hoses[index].GetComponent<HoseBurstMonitor>();
                _profiles[index] = new float[_hoses[index].Config.ParticleCount];
                if (_hoses[index] == _sourceHose)
                    _sourceHoseIndex = index;
            }

            _junctionIds = new int[_joints.Length];
            _splitterPartIds = new int[_joints.Length];
            var nextSplitterPartId = SourcePartId + 1 + _hoses.Length;
            for (var index = 0; index < _joints.Length; index++)
            {
                _junctionIds[index] = _world.AddJunction(_junctionBreakDistance);
                foreach (var end in _joints[index].Ends)
                    _world.Attach(_junctionIds[index], new RopeEnd(HoseIndex(end.Hose), end.End));

                _splitterPartIds[index] = _joints[index].IsSplitter ? nextSplitterPartId++ : -1;
            }
        }

        private IEnumerator SolveLoop()
        {
            var interval = new WaitForSeconds(1f / _solvesPerSecond);
            while (true)
            {
                Solve();
                yield return interval;
            }
        }

        private void Solve()
        {
            _graph.Clear();
            _graph.AddSource(_source.CurrentPressure);
            foreach (var hose in _hoses)
            {
                var config = hose.Config;
                _graph.AddHose(config.Length, config.LossPerMeter);
            }

            foreach (var joint in _joints)
            {
                if (joint.IsSplitter)
                    _graph.AddSplitter();
            }

            _graph.Link(SourcePartId, PressureGraph.SourceOutlet, HosePartId(_sourceHoseIndex), PressureGraph.HoseStart);
            LinkJoints();
            AddHoseMarkers();

            PressureSolver.Solve(_graph, _result);
            PublishProfiles();
        }

        private void LinkJoints()
        {
            for (var index = 0; index < _joints.Length; index++)
            {
                if (!_world.IsIntact(_junctionIds[index])) continue;

                var ends = _joints[index].Ends;
                if (!_joints[index].IsSplitter)
                {
                    LinkCoupling(ends[0], ends[1]);
                    continue;
                }

                for (var port = 0; port < ends.Count; port++)
                    LinkSplitterPort(ends[port], _splitterPartIds[index], port);
            }
        }

        private void LinkCoupling(HoseEndRef first, HoseEndRef second)
        {
            _graph.Link(PartIdOf(first.Hose), GraphEnd(first.End), PartIdOf(second.Hose), GraphEnd(second.End));
        }

        private void LinkSplitterPort(HoseEndRef end, int splitterPartId, int port)
        {
            _graph.Link(PartIdOf(end.Hose), GraphEnd(end.End), splitterPartId, port);
        }

        private void AddHoseMarkers()
        {
            for (var index = 0; index < _hoses.Length; index++)
            {
                var partId = HosePartId(index);
                PinchRuns.CollectPositions(_sensors[index].Pinched, _pinchPositions);
                foreach (var position in _pinchPositions)
                    _graph.Pinch(partId, position);

                if (_monitors[index].HasBurst)
                {
                    var config = _hoses[index].Config;
                    _graph.Leak(partId, config.BurstWeakPointFraction, config.BurstLeakLoss);
                }
            }
        }

        private void PublishProfiles()
        {
            for (var index = 0; index < _hoses.Length; index++)
            {
                var maxPressure = _hoses[index].Config.MaxPressure;
                var profile = _profiles[index];
                for (var particle = 0; particle < profile.Length; particle++)
                    profile[particle] = _result.Sample(HosePartId(index), (float)particle / (profile.Length - 1)) / maxPressure;

                _hoses[index].SetPressureProfile(profile);
            }
        }

        private int HoseIndex(HoseSegment hose) => Array.IndexOf(_hoses, hose);

        private int PartIdOf(HoseSegment hose) => HosePartId(HoseIndex(hose));

        private static int HosePartId(int hoseIndex) => SourcePartId + 1 + hoseIndex;

        private static int GraphEnd(RopeEndKind end) => end == RopeEndKind.Start ? PressureGraph.HoseStart : PressureGraph.HoseEnd;
    }
}
