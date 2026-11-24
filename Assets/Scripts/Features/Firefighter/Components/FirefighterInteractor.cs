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

            var eye = _firefighter.Eye.transform;
            var heldEnd = HeldEnd(out var hasHeldEnd);
            var connectTarget = default(HoseEndRef);
            var canConnect = hasHeldEnd && _network.IsEndFree(heldEnd) &&
                             _network.TryFindFreeEnd(_network.EndPosition(heldEnd), _config.ConnectRadiusMeters, heldEnd, out connectTarget);
            var hasBurst = _network.TryFindBurst(eye.position, eye.forward, _config.ReachMeters, _config.AimRadiusMeters, out var burst);
            var joint = _network.FindJointAlongRay(eye.position, eye.forward, _config.ReachMeters, _config.AimRadiusMeters);
            var valve = FindValve(eye);

            switch (UseResolver.Resolve(new UseContext(canConnect, hasBurst, joint != HoseNetwork.NoJoint, valve)))
            {
                case UseAction.Connect:
                    _grabber.Drop();
                    _network.Connect(heldEnd, connectTarget);
                    break;
                case UseAction.Tape:
                    burst.Tape();
                    break;
                case UseAction.Disconnect:
                    _network.Disconnect(joint);
                    break;
                case UseAction.ToggleValve:
                    valve.IsOpen = !valve.IsOpen;
                    break;
            }
        }

        private HoseEndRef HeldEnd(out bool hasHeldEnd)
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
    }
}
