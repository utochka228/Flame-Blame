using System;
using FlameBlame.Firefighter.Config;
using FlameBlame.Firefighter.Simulation;
using FlameBlame.Water.Components;
using UnityEngine;

namespace FlameBlame.Firefighter.Components
{
    [RequireComponent(typeof(Rigidbody), typeof(CapsuleCollider))]
    public class FirefighterBody : MonoBehaviour
    {
        [SerializeField] private FirefighterConfigSO _config;
        [SerializeField] private Camera _eye;
        [SerializeField] private PuddleField _puddles;

        private Rigidbody _body;
        private CapsuleCollider _capsule;
        private LookState _look;
        private FirefighterMotion _motion;
        private Vector2 _move;
        private bool _jumpRequested;

        public Camera Eye => _eye;
        public bool IsMotionOverridden { get; set; }
        public float MoveSpeedScale { get; set; } = 1f;

        public event Action<FirefighterCommands> Driven;

        private void Awake()
        {
            _body = GetComponent<Rigidbody>();
            _capsule = GetComponent<CapsuleCollider>();
            _body.freezeRotation = true;
            _look = new LookState(_config.Look.DegreesPerPixel, _config.Look.PitchLimitDegrees, transform.eulerAngles.y);
            _motion = new FirefighterMotion(_config.Movement.MoveSpeed, _config.Movement.Acceleration, _config.Movement.Deceleration, _config.Movement.JumpSpeed, _config.Movement.AirControlFraction);
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
            var facing = Quaternion.Euler(0f, _look.Yaw, 0f);
            _body.MoveRotation(facing);
            if (IsMotionOverridden) return;

            var isGrounded = IsGrounded();
            var traction = _puddles ? _puddles.TractionAt(transform.position) : 1f;
            var velocity = _motion.Step(_body.linearVelocity, _move, facing, Time.fixedDeltaTime, isGrounded, MoveSpeedScale, traction);
            if (_jumpRequested && isGrounded)
                velocity = _motion.Jump(velocity);

            _jumpRequested = false;
            _body.linearVelocity = Vector3.ClampMagnitude(velocity, _config.Movement.MaxSpeed);
        }

        public void Drive(FirefighterCommands commands)
        {
            _move = commands.Move;
            _jumpRequested |= commands.WasPressed(FirefighterButtons.Jump);
            _look.Apply(commands.Look);
            _eye.transform.localRotation = Quaternion.Euler(_look.Pitch, 0f, 0f);
            Driven?.Invoke(commands);
        }

        private void OnConfigChanged()
        {
            var movement = _config.Movement;
            _look.Configure(_config.Look.DegreesPerPixel, _config.Look.PitchLimitDegrees);
            _motion = new FirefighterMotion(movement.MoveSpeed, movement.Acceleration, movement.Deceleration, movement.JumpSpeed,
                movement.AirControlFraction);
        }

        private bool IsGrounded()
        {
            var bounds = _capsule.bounds;
            var origin = new Vector3(bounds.center.x, bounds.min.y + _capsule.radius, bounds.center.z);
            return Physics.CheckSphere(origin + Vector3.down * _config.Movement.GroundCheckDistance, _capsule.radius * 0.9f,
                _config.Movement.GroundLayers,
                QueryTriggerInteraction.Ignore);
        }
    }
}
