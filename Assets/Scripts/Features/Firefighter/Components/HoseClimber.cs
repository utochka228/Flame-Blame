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

        private Rigidbody _body;
        private RopeClimb _climb;
        private HoseSegment _hose;
        private float _climbInput;

        public bool IsClimbing => _hose;

        private void Awake()
        {
            _body = GetComponent<Rigidbody>();
            _climb = new RopeClimb(_config.ClimbSpeed);
        }

        private void OnEnable()
        {
            _firefighter.Driven += OnDriven;
        }

        private void OnDisable()
        {
            _firefighter.Driven -= OnDriven;
        }

        private void FixedUpdate()
        {
            if (!IsClimbing) return;

            var point = _climb.Step(_hose.Rope.Positions, _climbInput, Time.fixedDeltaTime);
            if (_climb.IsAtTop && _climbInput > 0f)
            {
                Mantle();
                return;
            }

            var target = point - transform.forward * _config.ClimbStandOffMeters;
            _body.linearVelocity = Vector3.ClampMagnitude((target - _body.position) / Time.fixedDeltaTime, _config.MaxSpeed);
        }

        public bool TryFindRope(Transform eye, out RopeParticle particle)
        {
            particle = _network.World.FindAlongRay(eye.position, eye.forward, _config.ReachMeters, _config.AimRadiusMeters);
            return !particle.IsNone && _network.HoseOf(particle.Rope).PressureFractions[particle.Index] <= _config.ClimbMaxPressureFraction;
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
            _firefighter.IsMotionOverridden = false;
            _body.useGravity = true;
        }

        private void Mantle()
        {
            var across = _climb.TopDirection(_hose.Rope.Positions);
            var heading = across.sqrMagnitude > 0f ? across : transform.forward;
            Stop();
            _body.linearVelocity = Vector3.up * _config.JumpSpeed + heading * _config.MoveSpeed;
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
