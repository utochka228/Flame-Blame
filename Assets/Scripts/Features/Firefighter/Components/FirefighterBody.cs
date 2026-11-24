using System;
using FlameBlame.Firefighter.Config;
using FlameBlame.Firefighter.Simulation;
using UnityEngine;

namespace FlameBlame.Firefighter.Components
{
    [RequireComponent(typeof(Rigidbody), typeof(CapsuleCollider))]
    public class FirefighterBody : MonoBehaviour
    {
        [SerializeField] private FirefighterConfigSO _config;
        [SerializeField] private Camera _eye;

        private Rigidbody _body;
        private CapsuleCollider _capsule;
        private LookState _look;
        private FirefighterMotion _motion;
        private Vector2 _move;
        private bool _jumpRequested;

        public Camera Eye => _eye;

        public event Action<FirefighterCommands> Driven;

        private void Awake()
        {
            _body = GetComponent<Rigidbody>();
            _capsule = GetComponent<CapsuleCollider>();
            _body.freezeRotation = true;
            _look = new LookState(_config.LookDegreesPerPixel, _config.PitchLimitDegrees, transform.eulerAngles.y);
            _motion = new FirefighterMotion(_config.MoveSpeed, _config.Acceleration, _config.JumpSpeed);
        }

        private void FixedUpdate()
        {
            var facing = Quaternion.Euler(0f, _look.Yaw, 0f);
            var velocity = _motion.Step(_body.linearVelocity, _move, facing, Time.fixedDeltaTime);
            if (_jumpRequested && IsGrounded())
                velocity = _motion.Jump(velocity);

            _jumpRequested = false;
            _body.linearVelocity = velocity;
            _body.MoveRotation(facing);
        }

        public void Drive(FirefighterCommands commands)
        {
            _move = commands.Move;
            _jumpRequested |= commands.WasPressed(FirefighterButtons.Jump);
            _look.Apply(commands.Look);
            _eye.transform.localRotation = Quaternion.Euler(_look.Pitch, 0f, 0f);
            Driven?.Invoke(commands);
        }

        private bool IsGrounded()
        {
            var bounds = _capsule.bounds;
            var origin = new Vector3(bounds.center.x, bounds.min.y + _capsule.radius, bounds.center.z);
            return Physics.CheckSphere(origin + Vector3.down * _config.GroundCheckDistance, _capsule.radius * 0.9f, _config.GroundLayers,
                QueryTriggerInteraction.Ignore);
        }
    }
}
