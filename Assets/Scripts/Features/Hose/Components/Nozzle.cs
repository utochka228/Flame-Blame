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

        [SerializeField] private NozzleConfigSO _config;
        [SerializeField] private HoseSegment _hose;
        [SerializeField] private HoseNetwork _network;
        [SerializeField] private ParticleSystem _jet;
        [SerializeField] private PuddleField _puddles;
        [SerializeField] private RopeEndKind _tip = RopeEndKind.Tail;

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
            Push(_hose.Rope.Positions[TipIndex], direction);
        }

        private void LateUpdate()
        {
            var positions = _hose.Positions;
            _jet.transform.SetPositionAndRotation(positions[TipIndex], Quaternion.LookRotation(JetDirection(positions)));
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

        private void Push(Vector3 origin, Vector3 direction)
        {
            if (_output.FlowFraction < MinVisibleFlowFraction) return;

            if (!Physics.SphereCast(origin, _config.PushRadius, direction, out var hit, _output.JetLength, _config.PushLayers,
                    QueryTriggerInteraction.Ignore))
                return;

            var water = _config.JetWaterPerSecond * _output.FlowFraction * Time.fixedDeltaTime;
            if (_puddles)
                _puddles.AddWater(hit.point, water);

            var wettable = hit.collider.GetComponentInParent<IWettable>();
            wettable?.Soak(water);

            if (hit.rigidbody)
                hit.rigidbody.AddForceAtPosition(direction * (_config.PushForce * _output.FlowFraction), hit.point);
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
