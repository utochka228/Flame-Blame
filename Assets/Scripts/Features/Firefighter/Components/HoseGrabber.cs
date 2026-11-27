using System.Collections.Generic;
using FlameBlame.Core.Events;
using FlameBlame.Firefighter.Config;
using FlameBlame.Firefighter.Events;
using FlameBlame.Firefighter.Simulation;
using FlameBlame.Hose.Components;
using FlameBlame.Hose.Simulation;
using UnityEngine;

namespace FlameBlame.Firefighter.Components
{
    public class HoseGrabber : MonoBehaviour
    {
        private const int NoGrip = -1;

        private static readonly List<HoseGrabber> Enabled = new List<HoseGrabber>();

        [SerializeField] private FirefighterConfigSO _config;
        [SerializeField] private FirefighterBody _firefighter;
        [SerializeField] private HoseNetwork _network;
        [SerializeField] private HoseSockets _sockets;

        private Rigidbody _body;
        private HoseTie _tie;
        private HoseClimber _climber;
        private int _grip = NoGrip;
        private int _aimGrip = NoGrip;
        private HoseSegment _heldHose;
        private Nozzle _heldNozzle;
        private HoldTracker _hold;
        private Vector3 _smoothedPull;

        public RopeParticle HeldParticle => IsHolding ? _network.World.GripParticle(_grip) : RopeParticle.None;

        private bool IsHolding => _grip != NoGrip;
        private bool IsHoldingNozzle => _aimGrip != NoGrip;
        private int GripCount => (IsHolding ? 1 : 0) + (IsHoldingNozzle ? 1 : 0);

        private void Awake()
        {
            _body = GetComponent<Rigidbody>();
            _tie = GetComponent<HoseTie>();
            _climber = GetComponent<HoseClimber>();
            _hold = new HoldTracker(_config.Grip.FollowSeconds, _config.Grip.MaxDistanceMeters);
        }

        private void OnEnable()
        {
            Enabled.Add(this);
            _firefighter.Driven += OnDriven;
            _config.Changed += OnConfigChanged;
        }

        private void OnDisable()
        {
            Enabled.Remove(this);
            _firefighter.Driven -= OnDriven;
            _config.Changed -= OnConfigChanged;
        }

        private void FixedUpdate()
        {
            if (!IsHolding) return;

            var eye = _firefighter.Eye.transform;
            var eyePosition = PhysicsEyePosition(eye);
            var heldParticle = _network.World.GripParticle(_grip);
            var heldPosition = _heldHose.Rope.Positions[heldParticle.Index];
            if (_hold.IsTooFar(heldPosition, eyePosition))
            {
                ReleaseGrips();
                return;
            }

            var previousHold = _hold.Position;
            _hold.Step(HoldPoint(eyePosition, eye.forward), eye.forward, Time.fixedDeltaTime);
            var radius = _heldHose.Config.Rope.ParticleRadius;
            _hold.Constrain(SphereSweep.Slide(previousHold, _hold.Position, radius, _heldHose.Config.Rope.CollisionLayers));
            _network.World.SetGripTarget(_grip, _hold.Position);
            if (IsHoldingNozzle)
            {
                _network.World.SetGripTarget(_aimGrip, _hold.Position - _hold.AimDirection * _heldNozzle.Spacing);
                _body.AddForce(_heldNozzle.RecoilAcceleration * _config.Grip.RecoilTransferMultiplier, ForceMode.Acceleration);
            }

            var gripsOnHose = HandGripsOn(heldParticle.Rope);
            _firefighter.MoveSpeedScale = CarryLoad.SpeedScale(gripsOnHose, IsHoldingNozzle, _heldHose.Config.Held.SoloCarrySpeedFraction);

            var pull = _network.World.GripTension(_grip) * _config.Grip.PullAccelerationMultiplier;
            _smoothedPull = Vector3.Lerp(_smoothedPull, pull, Time.fixedDeltaTime / _config.Grip.PullSmoothingSeconds);
            _body.AddForce(Vector3.ClampMagnitude(_smoothedPull, _config.Grip.MaxPullAcceleration), ForceMode.Acceleration);
        }

        public void Drop()
        {
            if (IsHolding)
                ReleaseGrips();
        }

        private void OnConfigChanged()
        {
            _hold.Configure(_config.Grip.FollowSeconds, _config.Grip.MaxDistanceMeters);
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

            if (_climber.IsClimbing)
                _climber.Stop();

            var eye = _firefighter.Eye.transform;
            var found = FindTarget(eye);
            if (found.IsNone)
                return;

            _tie.UntieAt(found, _config.Tie.GrabToleranceParticles);
            _sockets.FreeAt(found, _config.Tie.GrabToleranceParticles);
            _sockets.Disarm(found.Rope);

            _grip = _network.World.AddGrip(found, _config.Grip.Compliance);
            _heldHose = _network.HoseOf(found.Rope);
            _heldHose.SetHeld(found.Index);
            _hold.Begin(_heldHose.Rope.Positions[found.Index], eye.forward);
            EventBus.Publish(new HoseGrabbed(_heldHose.Rope.Positions[found.Index]));
            var nozzle = _network.NozzleOf(found.Rope);
            
            if (!nozzle || !nozzle.IsTip(found))
                return;

            _heldNozzle = nozzle;
            _aimGrip = _network.World.AddGrip(nozzle.InnerParticle, _config.Grip.AimCompliance);
            _hold.Begin(_heldHose.Rope.Positions[found.Index], nozzle.CurrentDirection);
        }

        private RopeParticle FindTarget(Transform eye)
        {
            return _network.TryFindNozzleTip(eye.position, eye.forward, _config.Interaction.ReachMeters, _config.Interaction.AimRadiusMeters,
                out var nozzle)
                ? nozzle.TipParticle
                : _network.World.FindAlongRay(eye.position, eye.forward, _config.Interaction.ReachMeters, _config.Interaction.AimRadiusMeters);
        }

        private void Throw()
        {
            var held = _network.World.GripParticle(_grip);
            EventBus.Publish(new HoseThrown(_heldHose.Rope.Positions[held.Index]));
            var velocity = _hold.Velocity * _config.Throw.InheritMultiplier + _firefighter.Eye.transform.forward * _config.Throw.Speed;
            ReleaseGrips();
            ImpartVelocity(held, velocity);
            _sockets.ArmCatch(held.Rope);
        }

        private void ImpartVelocity(RopeParticle held, Vector3 velocity)
        {
            var spread = _config.Throw.SpreadParticles;
            var count = _heldHose.Rope.Positions.Count;
            for (var offset = -spread; offset <= spread; offset++)
            {
                var index = held.Index + offset;
                if (index < 0 || index >= count) continue;

                var weight = ThrowSpread.Weight(offset, spread);
                _network.World.AddImpulse(new RopeParticle(held.Rope, index), velocity * weight);
            }
        }

        private static int HandGripsOn(int rope)
        {
            var count = 0;
            foreach (var grabber in Enabled)
            {
                if (grabber.HeldParticle.Rope == rope)
                    count += grabber.GripCount;
            }

            return count;
        }

        private Vector3 PhysicsEyePosition(Transform eye)
        {
            return _body.position + (eye.position - transform.position);
        }

        private Vector3 HoldPoint(Vector3 eyePosition, Vector3 forward)
        {
            var distance = _config.Grip.HoldDistanceMeters;
            var config = _heldHose.Config;
            if (Physics.SphereCast(eyePosition, config.Rope.ParticleRadius, forward, out var hit, distance, config.Rope.CollisionLayers,
                    QueryTriggerInteraction.Ignore))
                distance = hit.distance;

            return eyePosition + forward * distance;
        }

        private void ReleaseGrips()
        {
            _network.World.Release(_grip);
            _grip = NoGrip;
            _smoothedPull = Vector3.zero;
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
