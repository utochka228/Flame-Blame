using System;
using System.Collections;
using System.Collections.Generic;
using FlameBlame.Core.Extensions;
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
        public const int NoJoint = -1;

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

        private readonly List<RuntimeJoint> _runtimeJoints = new List<RuntimeJoint>();

        private RopeWorld _world;
        private HosePinchSensor[] _sensors;
        private HoseBurstMonitor[] _monitors;
        private Nozzle[] _nozzles;
        private float[][] _profiles;
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

        public int RopeIndex(HoseSegment hose) => HoseIndex(hose);

        public HoseSegment HoseOf(int ropeIndex) => _hoses[ropeIndex];

        public Vector3 JointPosition(int jointIndex) => _world.JunctionPosition(_runtimeJoints[jointIndex].Junction);

        public bool IsEndFree(HoseEndRef end)
        {
            if (end.Hose == _sourceHose && end.End == RopeEndKind.Start) return false;

            foreach (var joint in _runtimeJoints)
            {
                if (!IsLive(joint)) continue;

                foreach (var jointEnd in joint.Ends)
                {
                    if (jointEnd.Hose == end.Hose && jointEnd.End == end.End) return false;
                }
            }

            return true;
        }

        public Vector3 EndPosition(HoseEndRef end)
        {
            var positions = end.Hose.Rope.Positions;
            return positions[end.End == RopeEndKind.Start ? 0 : positions.Count - 1];
        }

        public bool TryFindFreeEnd(Vector3 point, float radius, HoseEndRef except, out HoseEndRef found)
        {
            found = default;
            var bestDistance = radius;
            foreach (var hose in _hoses)
            {
                foreach (var kind in new[] { RopeEndKind.Start, RopeEndKind.Tail })
                {
                    var candidate = new HoseEndRef(hose, kind);
                    if (hose == except.Hose && kind == except.End) continue;
                    if (!IsEndFree(candidate)) continue;

                    var distance = Vector3.Distance(EndPosition(candidate), point);
                    if (distance >= bestDistance) continue;

                    bestDistance = distance;
                    found = candidate;
                }
            }

            return bestDistance < radius;
        }

        public int FindJointAlongRay(Vector3 origin, Vector3 direction, float reach, float aimRadius)
        {
            var best = NoJoint;
            var bestOffset = aimRadius;
            for (var index = 0; index < _runtimeJoints.Count; index++)
            {
                if (!IsLive(_runtimeJoints[index])) continue;

                var offset = JointPosition(index).OffsetFromRay(origin, direction, reach);
                if (offset >= bestOffset) continue;

                bestOffset = offset;
                best = index;
            }

            return best;
        }

        public bool TryFindBurst(Vector3 origin, Vector3 direction, float reach, float aimRadius, out HoseBurstMonitor found)
        {
            for (var index = 0; index < _hoses.Length; index++)
            {
                if (!_monitors[index].HasBurst) continue;

                var weakPoint = new RopeParticle(index, _monitors[index].WeakPointIndex);
                if (!_world.IsAlongRay(weakPoint, origin, direction, reach, aimRadius)) continue;

                found = _monitors[index];
                return true;
            }

            found = null;
            return false;
        }

        public int Connect(HoseEndRef first, HoseEndRef second)
        {
            if (!IsEndFree(first) || !IsEndFree(second) || LiveJointCount() + _hoses.Length >= _maxPieces) return NoJoint;

            var joint = new RuntimeJoint(new[] { first, second }, AddJunction(new[] { first, second }));
            var slot = _runtimeJoints.FindIndex(existing => !existing.IsActive);
            if (slot < 0)
            {
                _runtimeJoints.Add(joint);
                return _runtimeJoints.Count - 1;
            }

            _runtimeJoints[slot] = joint;
            return slot;
        }

        public void Disconnect(int joint)
        {
            _world.RemoveJunction(_runtimeJoints[joint].Junction);
            _runtimeJoints[joint].IsActive = false;
        }

        private void BuildWorld()
        {
            _world = new RopeWorld(_hoses[0].Config.Substeps) { JunctionIterations = _junctionIterations };
            _sensors = new HosePinchSensor[_hoses.Length];
            _monitors = new HoseBurstMonitor[_hoses.Length];
            _nozzles = new Nozzle[_hoses.Length];
            _profiles = new float[_hoses.Length][];
            for (var index = 0; index < _hoses.Length; index++)
            {
                _world.AddRope(_hoses[index].Rope);
                _hoses[index].IsSteppedByNetwork = true;
                _sensors[index] = _hoses[index].GetComponent<HosePinchSensor>();
                _monitors[index] = _hoses[index].GetComponent<HoseBurstMonitor>();
                _nozzles[index] = _hoses[index].GetComponent<Nozzle>();
                _profiles[index] = new float[_hoses[index].Config.ParticleCount];
                if (_hoses[index] == _sourceHose)
                    _sourceHoseIndex = index;
            }

            foreach (var joint in _joints)
                _runtimeJoints.Add(new RuntimeJoint(joint.Ends, AddJunction(joint.Ends)));
        }

        private int AddJunction(IReadOnlyList<HoseEndRef> ends)
        {
            var junction = _world.AddJunction(_junctionBreakDistance);
            foreach (var end in ends)
                _world.Attach(junction, new RopeEnd(HoseIndex(end.Hose), end.End));

            return junction;
        }

        private bool IsLive(RuntimeJoint joint) => joint.IsActive && _world.IsIntact(joint.Junction);

        private int LiveJointCount()
        {
            var count = 0;
            foreach (var joint in _runtimeJoints)
            {
                if (IsLive(joint))
                    count++;
            }

            return count;
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

            foreach (var joint in _runtimeJoints)
            {
                if (IsLive(joint) && joint.IsSplitter)
                    joint.SplitterPart = _graph.AddSplitter();
            }

            _graph.Link(SourcePartId, PressureGraph.SourceOutlet, HosePartId(_sourceHoseIndex), PressureGraph.HoseStart);
            LinkJoints();
            AddHoseMarkers();
            AddThumbs();

            PressureSolver.Solve(_graph, _result);
            PublishProfiles();
            PublishNozzles();
        }

        private void LinkJoints()
        {
            foreach (var joint in _runtimeJoints)
            {
                if (!IsLive(joint)) continue;

                if (!joint.IsSplitter)
                {
                    LinkCoupling(joint.Ends[0], joint.Ends[1]);
                    continue;
                }

                for (var port = 0; port < joint.Ends.Length; port++)
                    LinkSplitterPort(joint.Ends[port], joint.SplitterPart, port);
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

                if (_monitors[index].IsLeaking)
                {
                    var config = _hoses[index].Config;
                    _graph.Leak(partId, config.BurstWeakPointFraction, _monitors[index].HasBurst ? config.BurstLeakLoss : config.TapeLeakLoss);
                }
            }
        }

        private void AddThumbs()
        {
            for (var index = 0; index < _hoses.Length; index++)
            {
                if (_nozzles[index])
                    _graph.Thumb(HosePartId(index), _nozzles[index].ThumbFraction * _nozzles[index].ThumbBoostFraction);
            }
        }

        private void PublishNozzles()
        {
            for (var index = 0; index < _hoses.Length; index++)
            {
                if (_nozzles[index])
                    _nozzles[index].SetTipPressureFraction(_result.TipPressure(HosePartId(index)) / _hoses[index].Config.MaxPressure);
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

        private class RuntimeJoint
        {
            private const int SplitterEndCount = 3;

            public HoseEndRef[] Ends { get; }
            public int Junction { get; }
            public bool IsActive { get; set; } = true;
            public int SplitterPart { get; set; }
            public bool IsSplitter => Ends.Length == SplitterEndCount;

            public RuntimeJoint(IReadOnlyList<HoseEndRef> ends, int junction)
            {
                Ends = new HoseEndRef[ends.Count];
                for (var index = 0; index < ends.Count; index++)
                    Ends[index] = ends[index];

                Junction = junction;
            }
        }
    }
}
