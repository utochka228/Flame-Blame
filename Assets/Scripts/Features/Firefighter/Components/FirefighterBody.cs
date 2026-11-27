using System;
using FlameBlame.Core.Events;
using FlameBlame.Firefighter.Config;
using FlameBlame.Firefighter.Events;
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
        private JumpGate _jump;
        private SlipGate _slip;
        private Vector2 _move;
        private bool _isJumpHeld;
        private float _airborneFallSpeed;
        private bool _isMotionOverridden;

        public Camera Eye => _eye;
        public bool IsMotionOverridden
        {
            get => _isMotionOverridden;
            set
            {
                if (value == _isMotionOverridden)
                    return;

                _isMotionOverridden = value;
                _jump.Cancel();
                IsGrounded = false;
                _airborneFallSpeed = 0f;
            }
        }

        public float MoveSpeedScale { get; set; } = 1f;
        public bool IsGrounded { get; private set; }
        public bool IsFallen => _slip.IsFallen;
        public Vector3 Velocity => _body.linearVelocity;
        public Quaternion EyeLookRotation => Quaternion.Euler(_look.Pitch, Mathf.DeltaAngle(transform.eulerAngles.y, _look.Yaw), 0f);

        public event Action<FirefighterCommands> Driven;
        public event Action<float> Landed;
        public event Action Jumped;
        public event Action Slipped;

        private void Awake()
        {
            _body = GetComponent<Rigidbody>();
            _capsule = GetComponent<CapsuleCollider>();
            _body.freezeRotation = true;
            _look = new LookState(_config.Look.DegreesPerPixel, _config.Look.PitchLimitDegrees, transform.eulerAngles.y, _config.Look.IsInvertedY);
            _jump = new JumpGate(_config.Movement.JumpBufferSeconds, _config.Movement.CoyoteSeconds, _config.Movement.JumpLockoutSeconds);
            _slip = new SlipGate(_config.Movement.SlipSpeed, _config.Movement.FallSeconds, _config.Movement.SlipImmunitySeconds);
            _motion = CreateMotion(_config.Movement);
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

            var jumps = _jump.Step(IsTouchingGround(), Time.fixedDeltaTime);
            TrackLanding();
            var traction = _puddles ? _puddles.TractionAt(transform.position) : 1f;
            StepSlip(traction < 1f);
            var velocity = _motion.Step(_body.linearVelocity, _move, facing, Time.fixedDeltaTime, IsGrounded, MoveSpeedScale, traction);
            if (jumps && !IsFallen)
            {
                velocity = _motion.Jump(velocity);
                Jumped?.Invoke();
            }
            else if (!IsGrounded)
                velocity = _motion.AddAirGravity(velocity, Physics.gravity.y, _isJumpHeld, Time.fixedDeltaTime);

            _body.linearVelocity = Vector3.ClampMagnitude(velocity, _config.Movement.MaxSpeed);
        }

        public void Drive(FirefighterCommands commands)
        {
            if (IsFallen)
                commands = new FirefighterCommands(Vector2.zero, commands.Look, FirefighterButtons.None, FirefighterButtons.None);

            _move = commands.Move;
            _isJumpHeld = commands.IsHeld(FirefighterButtons.Jump);
            if (commands.WasPressed(FirefighterButtons.Jump))
                _jump.Press();

            _look.Apply(commands.Look);
            _eye.transform.localRotation = EyeLookRotation;
            Driven?.Invoke(commands);
        }

        private void OnConfigChanged()
        {
            var movement = _config.Movement;
            _look.Configure(_config.Look.DegreesPerPixel, _config.Look.PitchLimitDegrees, _config.Look.IsInvertedY);
            _jump.Configure(movement.JumpBufferSeconds, movement.CoyoteSeconds, movement.JumpLockoutSeconds);
            _slip.Configure(movement.SlipSpeed, movement.FallSeconds, movement.SlipImmunitySeconds);
            _motion = CreateMotion(movement);
        }

        private static FirefighterMotion CreateMotion(FirefighterMovementConfig movement) =>
            new(movement.MoveSpeed, movement.Acceleration, movement.Deceleration, movement.JumpSpeed, movement.AirControlFraction,
                movement.FallGravityMultiplier, movement.ShortJumpGravityMultiplier);

        private void StepSlip(bool isWet)
        {
            var velocity = _body.linearVelocity;
            var planarSpeed = new Vector2(velocity.x, velocity.z).magnitude;
            if (!_slip.Step(IsGrounded && isWet, planarSpeed, Time.fixedDeltaTime))
                return;

            _move = Vector2.zero;
            Slipped?.Invoke();
            EventBus.Publish(new FirefighterSlipped(transform.position));
        }

        private void TrackLanding()
        {
            var isGrounded = _jump.IsGrounded;
            if (!isGrounded)
                _airborneFallSpeed = Mathf.Max(_airborneFallSpeed, -_body.linearVelocity.y);
            else if (!IsGrounded)
            {
                Landed?.Invoke(_airborneFallSpeed);
                _airborneFallSpeed = 0f;
            }

            IsGrounded = isGrounded;
        }

        private bool IsTouchingGround()
        {
            var bounds = _capsule.bounds;
            var origin = new Vector3(bounds.center.x, bounds.min.y + _capsule.radius, bounds.center.z);
            return Physics.CheckSphere(origin + Vector3.down * _config.Movement.GroundCheckDistance, _capsule.radius * 0.9f,
                _config.Movement.GroundLayers,
                QueryTriggerInteraction.Ignore);
        }
    }
}
