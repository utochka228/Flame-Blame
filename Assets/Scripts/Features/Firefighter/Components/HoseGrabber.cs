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

        private Rigidbody _body;
        private int _grip = NoGrip;
        private int _aimGrip = NoGrip;
        private HoseSegment _heldHose;
        private Nozzle _heldNozzle;
        private HoldTracker _hold;

        public RopeParticle HeldParticle => IsHolding ? _network.World.GripParticle(_grip) : RopeParticle.None;

        private bool IsHolding => _grip != NoGrip;
        private bool IsHoldingNozzle => _aimGrip != NoGrip;

        private void Awake()
        {
            _body = GetComponent<Rigidbody>();
            _hold = new HoldTracker(_config.GripFollowSeconds, _config.MaxGripDistanceMeters);
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
            var heldParticle = _network.World.GripParticle(_grip);
            var heldPosition = _heldHose.Rope.Positions[heldParticle.Index];
            if (_hold.IsTooFar(heldPosition, eye.position))
            {
                ReleaseGrips();
                return;
            }

            _hold.Step(eye.position + eye.forward * _config.HoldDistanceMeters, eye.forward, Time.fixedDeltaTime);
            _network.World.SetGripTarget(_grip, _hold.Position);
            if (IsHoldingNozzle)
            {
                _network.World.SetGripTarget(_aimGrip, _hold.Position - _hold.AimDirection * _heldNozzle.Spacing);
                _body.AddForce(_heldNozzle.RecoilAcceleration * _config.RecoilTransferMultiplier, ForceMode.Acceleration);
            }

            var gripsOnHose = _network.World.ActiveGripCount(heldParticle.Rope);
            _firefighter.MoveSpeedScale = CarryLoad.SpeedScale(gripsOnHose, IsHoldingNozzle, _heldHose.Config.SoloCarrySpeedFraction);

            var pull = _network.World.GripTension(_grip) * _config.PullAccelerationMultiplier;
            _body.AddForce(Vector3.ClampMagnitude(pull, _config.MaxPullAcceleration), ForceMode.Acceleration);
        }

        public void Drop()
        {
            if (IsHolding)
                ReleaseGrips();
        }

        private void OnDriven(FirefighterCommands commands)
        {
            if (IsHoldingNozzle)
                _heldNozzle.ThumbFraction = commands.IsHeld(FirefighterButtons.Thumb) ? 1f : 0f;

            if (commands.WasPressed(FirefighterButtons.Throw) && IsHolding)
                Throw();
            else if (commands.WasPressed(FirefighterButtons.Grab))
                ToggleGrab();
        }

        private void ToggleGrab()
        {
            if (IsHolding)
            {
                ReleaseGrips();
                return;
            }

            var eye = _firefighter.Eye.transform;
            var found = FindTarget(eye);
            if (found.IsNone) 
                return;

            _grip = _network.World.AddGrip(found, _config.GripCompliance);
            _heldHose = _network.HoseOf(found.Rope);
            _heldHose.SetHeld(found.Index);
            _hold.Begin(_heldHose.Rope.Positions[found.Index], eye.forward);
            var nozzle = _network.NozzleOf(found.Rope);
            
            if (!nozzle || !nozzle.IsTip(found))
                return;

            _heldNozzle = nozzle;
            _aimGrip = _network.World.AddGrip(nozzle.InnerParticle, _config.AimGripCompliance);
            _hold.Begin(_heldHose.Rope.Positions[found.Index], nozzle.CurrentDirection);
        }

        private RopeParticle FindTarget(Transform eye)
        {
            return _network.TryFindNozzleTip(eye.position, eye.forward, _config.ReachMeters, _config.AimRadiusMeters,
                out var nozzle)
                ? nozzle.TipParticle
                : _network.World.FindAlongRay(eye.position, eye.forward, _config.ReachMeters, _config.AimRadiusMeters);
        }

        private void Throw()
        {
            var held = _network.World.GripParticle(_grip);
            ReleaseGrips();
            _network.World.AddImpulse(held, _firefighter.Eye.transform.forward * _config.ThrowSpeed);
        }

        private void ReleaseGrips()
        {
            _network.World.Release(_grip);
            _grip = NoGrip;
            _heldHose.ClearHeld();
            _firefighter.MoveSpeedScale = 1f;
            if (!IsHoldingNozzle) return;

            _network.World.Release(_aimGrip);
            _aimGrip = NoGrip;
            _heldNozzle.ThumbFraction = 0f;
            _heldNozzle = null;
        }
    }
}
