using FlameBlame.Core.Events;
using FlameBlame.Firefighter.Config;
using FlameBlame.Firefighter.Events;
using FlameBlame.Firefighter.Simulation;
using FlameBlame.Hose.Components;
using FlameBlame.Hose.Simulation;
using FlameBlame.Interactables.Components;
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
        [SerializeField] private HoseSockets _sockets;

        private UseAction _targetAction;

        private void OnEnable()
        {
            _firefighter.Driven += OnDriven;
        }

        private void OnDisable()
        {
            _firefighter.Driven -= OnDriven;
            ShowTarget(UseAction.None);
        }

        private void OnDriven(FirefighterCommands commands)
        {
            var targets = FindTargets();
            var action = Resolve(targets);
            ShowTarget(action);
            if (!commands.WasPressed(FirefighterButtons.Use) || action == UseAction.None) return;

            Execute(action, targets);
            EventBus.Publish(new InteractionUsed());
        }

        private void ShowTarget(UseAction action)
        {
            if (action == _targetAction) return;

            _targetAction = action;
            EventBus.Publish(new InteractionTargetChanged(action));
        }

        private UseAction Resolve(UseTargets targets)
        {
            var context = new UseContext(targets.CanConnect, targets.Burst, targets.Joint != HoseNetwork.NoJoint, targets.Valve)
                .WithTie(targets.CanTie, targets.TiedInReach)
                .WithSocket(targets.CanLatch, targets.LatchedInReach)
                .WithDoor(targets.HasDoor)
                .WithClimb(targets.CanClimb, _climber.IsClimbing);
            return UseResolver.Resolve(context);
        }

        private UseTargets FindTargets()
        {
            var eye = _firefighter.Eye.transform;
            var targets = new UseTargets
            {
                Joint = _network.FindJointAlongRay(eye.position, eye.forward, _config.Interaction.ReachMeters, _config.Interaction.AimRadiusMeters)
            };
            FindInteractables(eye, ref targets);
            _network.TryFindBurst(eye.position, eye.forward, _config.Interaction.ReachMeters, _config.Interaction.AimRadiusMeters, out var burst, out var breachIndex);
            targets.BurstMonitor = burst;
            targets.BreachIndex = breachIndex;
            FindConnectTargets(ref targets);
            FindTieTargets(eye, ref targets);
            FindSocketTargets(eye, ref targets);
            FindClimbTargets(eye, ref targets);
            return targets;
        }

        private void FindConnectTargets(ref UseTargets targets)
        {
            targets.HeldEnd = FindHeldEnd(out var hasHeldEnd);
            if (!hasHeldEnd || _network.IsNozzleEnd(targets.HeldEnd) || !_network.IsEndFree(targets.HeldEnd)) return;

            var heldPosition = _network.EndPosition(targets.HeldEnd);
            targets.CanConnect = _network.TryFindFreeEnd(heldPosition, _config.Interaction.ConnectRadiusMeters, targets.HeldEnd, out var connectTarget);
            targets.ConnectTarget = connectTarget;
        }

        private void FindTieTargets(Transform eye, ref UseTargets targets)
        {
            if (!_tie.TryFindTarget(eye, out var tieTarget)) return;

            targets.TieTarget = tieTarget;
            targets.TiedInReach = _tie.IsTied(tieTarget.Body);
            targets.CanTie = !targets.TiedInReach && !_grabber.HeldParticle.IsNone;
        }

        private void FindSocketTargets(Transform eye, ref UseTargets targets)
        {
            if (!_sockets.TryFindInReach(eye, out var socket)) return;

            var held = _grabber.HeldParticle;
            targets.Socket = socket;
            targets.LatchedInReach = socket.IsOccupied;
            targets.CanLatch = !targets.LatchedInReach && !held.IsNone && _sockets.Accepts(socket, held);
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
                    targets.BurstMonitor.Tape(targets.BreachIndex);
                    break;
                case UseAction.Disconnect:
                    _network.Disconnect(targets.Joint);
                    break;
                case UseAction.ToggleValve:
                    targets.ValveSource.IsOpen = !targets.ValveSource.IsOpen;
                    break;
                case UseAction.ToggleDoor:
                    targets.Door.Toggle();
                    break;
                case UseAction.Tie:
                    TieHeld(targets.TieTarget);
                    break;
                case UseAction.Untie:
                    _tie.Untie(targets.TieTarget.Body);
                    break;
                case UseAction.Latch:
                    LatchHeld(targets.Socket);
                    break;
                case UseAction.Unlatch:
                    _sockets.Free(targets.Socket);
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

        private void LatchHeld(HoseSocket socket)
        {
            var held = _grabber.HeldParticle;
            _grabber.Drop();
            _sockets.Latch(socket, held);
        }

        private HoseEndRef FindHeldEnd(out bool hasHeldEnd)
        {
            var held = _grabber.HeldParticle;
            var hose = held.IsNone ? null : _network.HoseOf(held.Rope);
            var lastIndex = hose ? hose.Rope.Positions.Count - 1 : 0;
            hasHeldEnd = hose && (held.Index == 0 || held.Index == lastIndex);
            return hasHeldEnd ? new HoseEndRef(hose, held.Index == 0 ? RopeEndKind.Start : RopeEndKind.Tail) : default;
        }

        private void FindInteractables(Transform eye, ref UseTargets targets)
        {
            if (!Physics.Raycast(eye.position, eye.forward, out var hit, _config.Interaction.ReachMeters, _config.Interaction.Layers, QueryTriggerInteraction.Ignore))
                return;

            targets.ValveSource = hit.collider.GetComponentInParent<PressureSource>();
            targets.Door = hit.collider.GetComponentInParent<Door>();
        }

        private struct UseTargets
        {
            public bool CanConnect { get; set; }
            public HoseEndRef HeldEnd { get; set; }
            public HoseEndRef ConnectTarget { get; set; }
            public HoseBurstMonitor BurstMonitor { get; set; }
            public int BreachIndex { get; set; }
            public int Joint { get; set; }
            public PressureSource ValveSource { get; set; }
            public Door Door { get; set; }
            public HoseTie.TieTarget TieTarget { get; set; }
            public bool CanTie { get; set; }
            public bool TiedInReach { get; set; }
            public HoseSocket Socket { get; set; }
            public bool CanLatch { get; set; }
            public bool LatchedInReach { get; set; }
            public RopeParticle ClimbParticle { get; set; }
            public bool CanClimb { get; set; }

            public bool Burst => BurstMonitor;
            public bool Valve => ValveSource;
            public bool HasDoor => Door;
        }
    }
}
