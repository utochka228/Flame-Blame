using FlameBlame.Firefighter.Config;
using FlameBlame.Firefighter.Simulation;
using FlameBlame.Hose.Components;
using FlameBlame.Hose.Simulation;
using UnityEngine;

namespace FlameBlame.Firefighter.Components
{
    public class HoseGrabber : MonoBehaviour
    {
        private const int NoGrip = -1;

        [SerializeField] private FirefighterConfigSO _config;
        [SerializeField] private FirefighterBody _firefighter;
        [SerializeField] private HoseNetwork _network;
        [SerializeField] private Nozzle _nozzle;

        private Rigidbody _body;
        private int _grip = NoGrip;
        private int _aimGrip = NoGrip;
        private HoseSegment _heldHose;
        private Vector3 _holdPosition;
        private Vector3 _holdVelocity;
        private Vector3 _aimDirection;

        private bool IsHolding => _grip != NoGrip;
        private bool IsHoldingNozzle => _aimGrip != NoGrip;

        private void Awake()
        {
            _body = GetComponent<Rigidbody>();
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
            if (!IsHolding) return;

            var eye = _firefighter.Eye.transform;
            if (IsHeldTooFar(eye.position))
            {
                Release();
                return;
            }

            var holdPoint = eye.position + eye.forward * _config.HoldDistanceMeters;
            _holdPosition = Vector3.SmoothDamp(_holdPosition, holdPoint, ref _holdVelocity, _config.GripFollowSeconds, Mathf.Infinity,
                Time.fixedDeltaTime);
            _network.World.SetGripTarget(_grip, _holdPosition);
            if (IsHoldingNozzle)
            {
                _aimDirection = Vector3.Slerp(_aimDirection, eye.forward, 1f - Mathf.Exp(-Time.fixedDeltaTime / _config.GripFollowSeconds));
                _network.World.SetGripTarget(_aimGrip, _holdPosition - _aimDirection * _nozzle.Spacing);
                _body.AddForce(_nozzle.RecoilAcceleration * _config.RecoilTransferMultiplier, ForceMode.Acceleration);
            }

            var pull = _network.World.GripTension(_grip) * _config.PullAccelerationMultiplier;
            _body.AddForce(Vector3.ClampMagnitude(pull, _config.MaxPullAcceleration), ForceMode.Acceleration);
        }

        private void OnDriven(FirefighterCommands commands)
        {
            _nozzle.ThumbFraction = IsHoldingNozzle && commands.IsHeld(FirefighterButtons.Thumb) ? 1f : 0f;
            if (commands.WasPressed(FirefighterButtons.Throw) && IsHolding)
                Throw();
            else if (commands.WasPressed(FirefighterButtons.Grab))
                ToggleGrab();
        }

        private void ToggleGrab()
        {
            if (IsHolding)
            {
                Release();
                return;
            }

            var eye = _firefighter.Eye.transform;
            var found = FindTarget(eye);
            if (found.IsNone) return;

            _grip = _network.World.AddGrip(found, _config.GripCompliance);
            _heldHose = _network.HoseOf(found.Rope);
            _heldHose.SetHeld(found.Index);
            _holdPosition = _heldHose.Rope.Positions[found.Index];
            _holdVelocity = Vector3.zero;
            if (!_nozzle.IsTip(found)) return;

            _aimGrip = _network.World.AddGrip(_nozzle.InnerParticle(), _config.AimGripCompliance);
            _aimDirection = _nozzle.CurrentDirection;
        }

        private bool IsHeldTooFar(Vector3 holderPosition)
        {
            var heldPosition = _heldHose.Rope.Positions[_network.World.GripParticle(_grip).Index];
            return Vector3.Distance(heldPosition, holderPosition) > _config.MaxGripDistanceMeters;
        }

        private RopeParticle FindTarget(Transform eye)
        {
            var tip = _nozzle.TipParticle();
            if (_network.World.IsAlongRay(tip, eye.position, eye.forward, _config.ReachMeters, _config.AimRadiusMeters))
                return tip;

            return _network.World.FindAlongRay(eye.position, eye.forward, _config.ReachMeters, _config.AimRadiusMeters);
        }

        private void Throw()
        {
            var held = _network.World.GripParticle(_grip);
            Release();
            _network.World.AddImpulse(held, _firefighter.Eye.transform.forward * _config.ThrowSpeed);
        }

        private void Release()
        {
            _network.World.Release(_grip);
            _grip = NoGrip;
            _heldHose.ClearHeld();
            if (!IsHoldingNozzle) return;

            _network.World.Release(_aimGrip);
            _aimGrip = NoGrip;
        }
    }
}
