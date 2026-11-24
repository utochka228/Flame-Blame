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
            var heldEnd = FindHeldEnd(out var hasHeldEnd);
            var connectTarget = default(HoseEndRef);
            var canConnect = hasHeldEnd && _network.IsEndFree(heldEnd) &&
                             _network.TryFindFreeEnd(_network.EndPosition(heldEnd), _config.ConnectRadiusMeters, heldEnd, out connectTarget);
            _network.TryFindBurst(eye.position, eye.forward, _config.ReachMeters, _config.AimRadiusMeters, out var burst);
            var joint = _network.FindJointAlongRay(eye.position, eye.forward, _config.ReachMeters, _config.AimRadiusMeters);
            var hasTieTarget = _tie.TryFindTarget(eye, out var tieTarget);
            var isTied = hasTieTarget && _tie.IsTied(tieTarget.Body);
            var climbParticle = RopeParticle.None;
            var canClimb = _grabber.HeldParticle.IsNone && _climber.TryFindRope(eye, out climbParticle);
            return new UseTargets
            {
                CanConnect = canConnect,
                HeldEnd = heldEnd,
                ConnectTarget = connectTarget,
                BurstMonitor = burst,
                Joint = joint,
                ValveSource = FindValve(eye),
                TieTarget = tieTarget,
                CanTie = hasTieTarget && !isTied && !_grabber.HeldParticle.IsNone,
                TiedInReach = isTied,
                ClimbParticle = climbParticle,
                CanClimb = canClimb
            };
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
                    var held = _grabber.HeldParticle;
                    _grabber.Drop();
                    _tie.Tie(held, targets.TieTarget);
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
