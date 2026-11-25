using FlameBlame.Firefighter.Config;
using FlameBlame.Firefighter.Simulation;
using FlameBlame.Hose.Components;
using FlameBlame.Hose.Simulation;
using FlameBlame.Pressure.Components;
using UnityEngine;

namespace FlameBlame.Firefighter.Components
{
    public class FirefighterInteractor : MonoBehaviour
    {
        [SerializeField] private FirefighterConfigSO _config;
        [SerializeField] private FirefighterBody _firefighter;
        [SerializeField] private HoseGrabber _grabber;
        [SerializeField] private HoseNetwork _network;
        [SerializeField] private HoseTie _tie;
        [SerializeField] private HoseClimber _climber;

        private void OnEnable()
        {
            _firefighter.Driven += OnDriven;
        }

        private void OnDisable()
        {
            _firefighter.Driven -= OnDriven;
        }

        private void OnDriven(FirefighterCommands commands)
        {
            if (!commands.WasPressed(FirefighterButtons.Use)) return;

            var targets = FindTargets();
            var context = new UseContext(targets.CanConnect, targets.Burst, targets.Joint != HoseNetwork.NoJoint, targets.Valve)
                .WithTie(targets.CanTie, targets.TiedInReach)
                .WithClimb(targets.CanClimb, _climber.IsClimbing);
            Execute(UseResolver.Resolve(context), targets);
        }

        private UseTargets FindTargets()
        {
            var eye = _firefighter.Eye.transform;
            var targets = new UseTargets
            {
                Joint = _network.FindJointAlongRay(eye.position, eye.forward, _config.ReachMeters, _config.AimRadiusMeters),
                ValveSource = FindValve(eye)
            };
            _network.TryFindBurst(eye.position, eye.forward, _config.ReachMeters, _config.AimRadiusMeters, out var burst);
            targets.BurstMonitor = burst;
            FindConnectTargets(ref targets);
            FindTieTargets(eye, ref targets);
            FindClimbTargets(eye, ref targets);
            return targets;
        }

        private void FindConnectTargets(ref UseTargets targets)
        {
            targets.HeldEnd = FindHeldEnd(out var hasHeldEnd);
            if (!hasHeldEnd || !_network.IsEndFree(targets.HeldEnd)) return;

            var heldPosition = _network.EndPosition(targets.HeldEnd);
            targets.CanConnect = _network.TryFindFreeEnd(heldPosition, _config.ConnectRadiusMeters, targets.HeldEnd, out var connectTarget);
            targets.ConnectTarget = connectTarget;
        }

        private void FindTieTargets(Transform eye, ref UseTargets targets)
        {
            if (!_tie.TryFindTarget(eye, out var tieTarget)) return;

            targets.TieTarget = tieTarget;
            targets.TiedInReach = _tie.IsTied(tieTarget.Body);
            targets.CanTie = !targets.TiedInReach && !_grabber.HeldParticle.IsNone;
        }

        private void FindClimbTargets(Transform eye, ref UseTargets targets)
        {
            if (!_grabber.HeldParticle.IsNone) return;

            targets.CanClimb = _climber.TryFindRope(eye, out var climbParticle);
            targets.ClimbParticle = climbParticle;
        }

        private void Execute(UseAction action, UseTargets targets)
        {
            switch (action)
            {
                case UseAction.Connect:
                    _grabber.Drop();
                    _network.Connect(targets.HeldEnd, targets.ConnectTarget);
                    break;
                case UseAction.Tape:
                    targets.BurstMonitor.Tape();
                    break;
                case UseAction.Disconnect:
                    _network.Disconnect(targets.Joint);
                    break;
                case UseAction.ToggleValve:
                    targets.ValveSource.IsOpen = !targets.ValveSource.IsOpen;
                    break;
                case UseAction.Tie:
                    TieHeld(targets.TieTarget);
                    break;
                case UseAction.Untie:
                    _tie.Untie(targets.TieTarget.Body);
                    break;
                case UseAction.Climb:
                    _climber.Begin(targets.ClimbParticle);
                    break;
                case UseAction.StopClimb:
                    _climber.Stop();
                    break;
            }
        }

        private void TieHeld(HoseTie.TieTarget target)
        {
            var held = _grabber.HeldParticle;
            _grabber.Drop();
            _tie.Tie(held, target);
        }

        private HoseEndRef FindHeldEnd(out bool hasHeldEnd)
        {
            var held = _grabber.HeldParticle;
            var hose = held.IsNone ? null : _network.HoseOf(held.Rope);
            var lastIndex = hose ? hose.Rope.Positions.Count - 1 : 0;
            hasHeldEnd = hose && (held.Index == 0 || held.Index == lastIndex);
            return hasHeldEnd ? new HoseEndRef(hose, held.Index == 0 ? RopeEndKind.Start : RopeEndKind.Tail) : default;
        }

        private PressureSource FindValve(Transform eye)
        {
            if (!Physics.Raycast(eye.position, eye.forward, out var hit, _config.ReachMeters, _config.InteractLayers, QueryTriggerInteraction.Ignore))
                return null;

            return hit.collider.GetComponentInParent<PressureSource>();
        }

        private struct UseTargets
        {
            public bool CanConnect { get; set; }
            public HoseEndRef HeldEnd { get; set; }
            public HoseEndRef ConnectTarget { get; set; }
            public HoseBurstMonitor BurstMonitor { get; set; }
            public int Joint { get; set; }
            public PressureSource ValveSource { get; set; }
            public HoseTie.TieTarget TieTarget { get; set; }
            public bool CanTie { get; set; }
            public bool TiedInReach { get; set; }
            public RopeParticle ClimbParticle { get; set; }
            public bool CanClimb { get; set; }

            public bool Burst => BurstMonitor;
            public bool Valve => ValveSource;
        }
    }
}
