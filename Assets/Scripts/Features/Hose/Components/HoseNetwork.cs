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
        public const int NoJoint = RopeJoints.NoJoint;

        private const int SourcePartId = 0;
        private const int SplitterEndCount = 3;
        private const int NoSplitter = -1;

        private static readonly RopeEndKind[] EndKinds = { RopeEndKind.Start, RopeEndKind.Tail };
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

        private readonly List<int> _splitterParts = new List<int>();

        private RopeWorld _world;
        private RopeJoints _ropeJoints;
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

        public Nozzle NozzleOf(int ropeIndex) => _nozzles[ropeIndex];

        public bool TryFindNozzleTip(Vector3 origin, Vector3 direction, float reach, float aimRadius, out Nozzle found)
        {
            foreach (var nozzle in _nozzles)
            {
                if (!nozzle || !_world.IsAlongRay(nozzle.TipParticle, origin, direction, reach, aimRadius)) continue;

                found = nozzle;
                return true;
            }

            found = null;
            return false;
        }

        public Vector3 JointPosition(int jointIndex) => _ropeJoints.Position(jointIndex);

        public bool IsEndFree(HoseEndRef end) => _ropeJoints.IsEndFree(ToRopeEnd(end));

        public bool IsNozzleEnd(HoseEndRef end)
        {
            var nozzle = _nozzles[HoseIndex(end.Hose)];
            return nozzle && nozzle.TipEnd == end.End;
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
                foreach (var kind in EndKinds)
                {
                    var candidate = new HoseEndRef(hose, kind);
                    if (hose == except.Hose && kind == except.End) continue;
                    if (!IsEndFree(candidate) || IsNozzleEnd(candidate)) continue;

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
            return _ropeJoints.FindAlongRay(origin, direction, reach, aimRadius);
        }

        public bool TryFindBurst(Vector3 origin, Vector3 direction, float reach, float aimRadius, out HoseBurstMonitor found, out int breachIndex)
        {
            for (var index = 0; index < _hoses.Length; index++)
            {
                foreach (var breach in _monitors[index].Breaches)
                {
                    if (breach.IsTaped) 
                        continue;

                    for (var particle = breach.FirstIndex; particle <= breach.LastIndex; particle++)
                    {
                        if (!_world.IsAlongRay(new RopeParticle(index, particle), origin, direction, reach, aimRadius)) 
                            continue;

                        found = _monitors[index];
                        breachIndex = particle;
                        return true;
                    }
                }
            }

            found = null;
            breachIndex = -1;
            return false;
        }

        public int Connect(HoseEndRef first, HoseEndRef second)
        {
            return _ropeJoints.Add(new[] { ToRopeEnd(first), ToRopeEnd(second) });
        }

        public void Disconnect(int joint)
        {
            _ropeJoints.Remove(joint);
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

            _ropeJoints = new RopeJoints(_world, _junctionBreakDistance, _maxPieces - _hoses.Length);
            _ropeJoints.Reserve(new RopeEnd(_sourceHoseIndex, RopeEndKind.Start));
            foreach (var joint in _joints)
                _ropeJoints.Add(ToRopeEnds(joint.Ends));
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

            _splitterParts.Clear();
            for (var joint = 0; joint < _ropeJoints.Count; joint++)
                _splitterParts.Add(_ropeJoints.IsLive(joint) && IsSplitter(joint) ? _graph.AddSplitter() : NoSplitter);

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
            for (var joint = 0; joint < _ropeJoints.Count; joint++)
            {
                if (!_ropeJoints.IsLive(joint)) continue;

                var ends = _ropeJoints.Ends(joint);
                if (!IsSplitter(joint))
                {
                    LinkCoupling(ends[0], ends[1]);
                    continue;
                }

                for (var port = 0; port < ends.Count; port++)
                    LinkSplitterPort(ends[port], _splitterParts[joint], port);
            }
        }

        private void LinkCoupling(RopeEnd first, RopeEnd second)
        {
            _graph.Link(HosePartId(first.Rope), GraphEnd(first.Kind), HosePartId(second.Rope), GraphEnd(second.Kind));
        }

        private void LinkSplitterPort(RopeEnd end, int splitterPartId, int port)
        {
            _graph.Link(HosePartId(end.Rope), GraphEnd(end.Kind), splitterPartId, port);
        }

        private void AddHoseMarkers()
        {
            for (var index = 0; index < _hoses.Length; index++)
            {
                var partId = HosePartId(index);
                PinchRuns.CollectPositions(_sensors[index].Pinched, _pinchPositions);
                foreach (var position in _pinchPositions)
                    _graph.Pinch(partId, position);

                var lastParticle = _hoses[index].Config.ParticleCount - 1;
                foreach (var breach in _monitors[index].Breaches)
                    _graph.Leak(partId, (float)breach.Index / lastParticle, breach.Loss);
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

        private bool IsSplitter(int joint) => _ropeJoints.Ends(joint).Count == SplitterEndCount;

        private RopeEnd ToRopeEnd(HoseEndRef end) => new RopeEnd(HoseIndex(end.Hose), end.End);

        private RopeEnd[] ToRopeEnds(IReadOnlyList<HoseEndRef> ends)
        {
            var ropeEnds = new RopeEnd[ends.Count];
            for (var index = 0; index < ends.Count; index++)
                ropeEnds[index] = ToRopeEnd(ends[index]);

            return ropeEnds;
        }

        private static int HosePartId(int hoseIndex) => SourcePartId + 1 + hoseIndex;

        private static int GraphEnd(RopeEndKind end) => end == RopeEndKind.Start ? PressureGraph.HoseStart : PressureGraph.HoseEnd;
    }
}
