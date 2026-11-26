using FlameBlame.Firefighter.Config;
using FlameBlame.Firefighter.Simulation;
using FlameBlame.Hose.Components;
using FlameBlame.Hose.Simulation;
using UnityEngine;

namespace FlameBlame.Firefighter.Components
{
    public class HoseClimber : MonoBehaviour
    {
        [SerializeField] private FirefighterConfigSO _config;
        [SerializeField] private FirefighterBody _firefighter;
        [SerializeField] private HoseNetwork _network;

        private readonly ClimbMantle _mantle = new ClimbMantle();

        private Rigidbody _body;
        private Collider _collider;
        private RopeClimb _climb;
        private HoseSegment _hose;
        private float _climbInput;
        private bool _isMantling;

        public bool IsClimbing => _hose;
        public bool IsMantling => _isMantling;

        private void Awake()
        {
            _body = GetComponent<Rigidbody>();
            _collider = GetComponentInChildren<Collider>();
            _climb = new RopeClimb(_config.Climb.Speed);
        }

        private void OnEnable()
        {
            _firefighter.Driven += OnDriven;
            _config.Changed += OnConfigChanged;
        }

        private void OnDisable()
        {
            _firefighter.Driven -= OnDriven;
            _config.Changed -= OnConfigChanged;
        }

        private void FixedUpdate()
        {
            if (!IsClimbing) return;

            if (_isMantling)
            {
                StepMantle();
                return;
            }

            var point = _climb.Step(_hose.Rope.Positions, _climbInput, Time.fixedDeltaTime, _firefighter.Eye.transform.forward);
            if (_climb.IsAtTop && _climb.EffectiveInput > 0f)
            {
                Mantle();
                return;
            }

            var target = point - transform.forward * _config.Climb.StandOffMeters;
            _body.linearVelocity = Vector3.ClampMagnitude((target - _body.position) / Time.fixedDeltaTime, _config.Movement.MaxSpeed);
        }

        public bool TryFindRope(Transform eye, out RopeParticle particle)
        {
            particle = _network.World.FindAlongRay(eye.position, eye.forward, _config.Interaction.ReachMeters, _config.Interaction.AimRadiusMeters);
            return !particle.IsNone && _network.HoseOf(particle.Rope).PressureFractions[particle.Index] <= _config.Climb.MaxPressureFraction;
        }

        public void Begin(RopeParticle particle)
        {
            _hose = _network.HoseOf(particle.Rope);
            _climb.Begin(particle.Index);
            _climbInput = 0f;
            _firefighter.IsMotionOverridden = true;
            _body.useGravity = false;
        }

        public void Stop()
        {
            _hose = null;
            _isMantling = false;
            _firefighter.IsMotionOverridden = false;
            _body.useGravity = true;
        }

        private void Mantle()
        {
            _mantle.Begin(_climb.Position(_hose.Rope.Positions).y);
            _isMantling = true;
        }

        private void StepMantle()
        {
            if (!_mantle.Step(_collider.bounds.min.y, Time.fixedDeltaTime))
            {
                _body.linearVelocity = Vector3.up * _config.Movement.JumpSpeed;
                return;
            }

            Stop();
            _body.linearVelocity = transform.forward * _config.Movement.MoveSpeed;
        }

        private void OnConfigChanged()
        {
            _climb.Configure(_config.Climb.Speed);
        }

        private void OnDriven(FirefighterCommands commands)
        {
            if (!IsClimbing) return;

            _climbInput = commands.Move.y;
            if (commands.WasPressed(FirefighterButtons.Jump))
                Stop();
        }
    }
}
