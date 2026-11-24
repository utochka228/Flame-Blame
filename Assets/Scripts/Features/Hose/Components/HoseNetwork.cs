using System.Collections;
using System.Collections.Generic;
using FlameBlame.Hose.Simulation;
using FlameBlame.Pressure.Components;
using FlameBlame.Pressure.Simulation;
using UnityEngine;

namespace FlameBlame.Hose.Components
{
    public class HoseNetwork : MonoBehaviour
    {
        private const int SourcePartId = 0;

        [SerializeField] private PressureSource _source;
        [SerializeField] private HoseSegment _sourceHose;
        [SerializeField] private HoseSegment[] _hoses;
        [SerializeField] private HoseJoint[] _joints;
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

        public Vector3 JointPosition(int jointIndex) => _world.JunctionPosition(_junctionIds[jointIndex]);

        private void Start()
        {
            BuildWorld();
            StartCoroutine(SolveLoop());
        }

        private void FixedUpdate()
        {
            _world.Step(Time.fixedDeltaTime);
        }

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
                _graph.AddHose(hose.Config.Length, hose.Config.LossPerMeter);

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
                if (_joints[index].IsSplitter)
                {
                    for (var port = 0; port < ends.Count; port++)
                        _graph.Link(HosePartId(HoseIndex(ends[port].Hose)), (int)ends[port].End, _splitterPartIds[index], port);
                }
                else
                {
                    _graph.Link(HosePartId(HoseIndex(ends[0].Hose)), (int)ends[0].End, HosePartId(HoseIndex(ends[1].Hose)), (int)ends[1].End);
                }
            }
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
                var config = _hoses[index].Config;
                var profile = _profiles[index];
                for (var particle = 0; particle < profile.Length; particle++)
                    profile[particle] = _result.Sample(HosePartId(index), (float)particle / (profile.Length - 1)) / config.MaxPressure;

                _hoses[index].SetPressureProfile(profile);
            }
        }

        private int HoseIndex(HoseSegment hose) => System.Array.IndexOf(_hoses, hose);

        private static int HosePartId(int hoseIndex) => SourcePartId + 1 + hoseIndex;
    }
}
