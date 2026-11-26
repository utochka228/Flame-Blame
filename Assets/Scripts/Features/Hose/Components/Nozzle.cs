using System.Collections.Generic;
using FlameBlame.Hose.Config;
using FlameBlame.Hose.Simulation;
using FlameBlame.Water;
using FlameBlame.Water.Components;
using UnityEngine;

namespace FlameBlame.Hose.Components
{
    public class Nozzle : MonoBehaviour
    {
        private const float MinVisibleFlowFraction = 0.02f;
        private const int ArcSegments = 6;

        [SerializeField] private NozzleConfigSO _config;
        [SerializeField] private HoseSegment _hose;
        [SerializeField] private HoseNetwork _network;
        [SerializeField] private ParticleSystem _jet;
        [SerializeField] private PuddleField _puddles;
        [SerializeField] private RopeEndKind _tip = RopeEndKind.Tail;
        [SerializeField] [Min(0f)] private float _muzzleOffsetMeters;

        private NozzleModel _model;
        private float _tipPressureFraction;
        private NozzleOutput _output;

        public float ThumbFraction { get; set; }
        public float ThumbBoostFraction => _config.ThumbBoostFraction;
        public Vector3 RecoilAcceleration { get; private set; }
        public float Spacing => _hose.Config.Rope.Spacing;
        public RopeEndKind TipEnd => _tip;
        public Vector3 TipPosition => _hose.Positions[TipIndex];
        public Vector3 CurrentDirection => JetDirection(_hose.Rope.Positions);
        public RopeParticle TipParticle => new RopeParticle(_network.RopeIndex(_hose), TipIndex);
        public RopeParticle InnerParticle => new RopeParticle(_network.RopeIndex(_hose), InnerIndex);

        private int TipIndex => _tip == RopeEndKind.Tail ? _hose.Rope.Positions.Count - 1 : 0;
        private int InnerIndex => _tip == RopeEndKind.Tail ? TipIndex - 1 : TipIndex + 1;

        private void Awake()
        {
            _model = _config.CreateModel();
        }

        private void OnEnable()
        {
            _config.Changed += OnConfigChanged;
        }

        private void OnDisable()
        {
            _config.Changed -= OnConfigChanged;
        }

        private void FixedUpdate()
        {
            _output = _model.Evaluate(_tipPressureFraction, ThumbFraction);
            var direction = JetDirection(_hose.Rope.Positions);
            RecoilAcceleration = -direction * _output.RecoilAcceleration;
            _network.World.AddImpulse(TipParticle, RecoilAcceleration * Time.fixedDeltaTime);
            Push(MuzzlePosition(_hose.Rope.Positions, direction), direction);
        }

        private void LateUpdate()
        {
            var positions = _hose.Positions;
            var direction = JetDirection(positions);
            _jet.transform.SetPositionAndRotation(MuzzlePosition(positions, direction), Quaternion.LookRotation(direction));
            DriveParticles();
        }

        public void SetTipPressureFraction(float tipPressureFraction)
        {
            _tipPressureFraction = tipPressureFraction;
        }

        public bool IsTip(RopeParticle particle)
        {
            return particle.Rope == _network.RopeIndex(_hose) && particle.Index == TipIndex;
        }

        private void OnConfigChanged()
        {
            _model = _config.CreateModel();
        }

        private Vector3 JetDirection(IReadOnlyList<Vector3> positions)
        {
            return (positions[TipIndex] - positions[InnerIndex]).normalized;
        }

        private Vector3 MuzzlePosition(IReadOnlyList<Vector3> positions, Vector3 direction)
        {
            return positions[TipIndex] + direction * _muzzleOffsetMeters;
        }

        private void Push(Vector3 origin, Vector3 direction)
        {
            if (_output.FlowFraction < MinVisibleFlowFraction) return;

            if (!CastAlongArc(origin, direction, out var hit))
                return;

            var water = _config.JetWaterPerSecond * _output.FlowFraction * Time.fixedDeltaTime;
            if (_puddles)
                _puddles.AddWater(hit.point, water);

            var wettable = hit.collider.GetComponentInParent<IWettable>();
            wettable?.Soak(water);

            if (hit.rigidbody)
                hit.rigidbody.AddForceAtPosition(direction * (_config.PushForce * _output.FlowFraction), hit.point);
        }

        private bool CastAlongArc(Vector3 origin, Vector3 direction, out RaycastHit hit)
        {
            var lifetime = _config.ParticleLifetime;
            var velocity = direction * (_output.JetLength / lifetime);
            var gravity = Physics.gravity * _jet.main.gravityModifier.constant;
            var segmentStart = origin;

            for (var segment = 1; segment <= ArcSegments; segment++)
            {
                var time = lifetime * segment / ArcSegments;
                var segmentEnd = origin + velocity * time + gravity * (0.5f * time * time);
                var step = segmentEnd - segmentStart;
                if (Physics.SphereCast(segmentStart, _config.PushRadius, step.normalized, out hit, step.magnitude, _config.PushLayers,
                        QueryTriggerInteraction.Ignore))
                    return true;

                segmentStart = segmentEnd;
            }

            hit = default;
            return false;
        }

        private void DriveParticles()
        {
            var hasFlow = _output.FlowFraction >= MinVisibleFlowFraction;
            var emission = _jet.emission;
            emission.rateOverTime = hasFlow ? _config.ParticlesPerSecondAtFullFlow * _output.FlowFraction : 0f;

            var main = _jet.main;
            main.startLifetime = _config.ParticleLifetime;
            main.startSpeed = _output.JetLength / _config.ParticleLifetime;

            var shape = _jet.shape;
            shape.angle = _output.SpreadDegrees * 0.5f;
        }
    }
}
