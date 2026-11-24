using FlameBlame.Firefighter.Config;
using FlameBlame.Firefighter.Simulation;
using FlameBlame.Hose.Components;
using FlameBlame.Hose.Simulation;
using UnityEngine;

namespace FlameBlame.Firefighter.Components
{
    public class HoseGrabber : MonoBehaviour
    {
        [SerializeField] private FirefighterConfigSO _config;
        [SerializeField] private FirefighterBody _firefighter;
        [SerializeField] private HoseNetwork _network;

        private Rigidbody _body;
        private RopeParticle _held = RopeParticle.None;
        private int _grip;

        private bool IsHolding => !_held.IsNone;

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
            _network.World.SetGripTarget(_grip, eye.position + eye.forward * _config.HoldDistanceMeters);
            var pull = _network.World.GripTension(_grip) * _config.PullAccelerationMultiplier;
            _body.AddForce(Vector3.ClampMagnitude(pull, _config.MaxPullAcceleration), ForceMode.Acceleration);
        }

        private void OnDriven(FirefighterCommands commands)
        {
            if (commands.ThrowPressed && IsHolding)
                Throw();
            else if (commands.GrabPressed)
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
            var found = _network.World.FindAlongRay(eye.position, eye.forward, _config.ReachMeters, _config.AimRadiusMeters);
            if (found.IsNone) return;

            _held = found;
            _grip = _network.World.AddGrip(found, _config.GripCompliance);
        }

        private void Throw()
        {
            var held = _held;
            Release();
            _network.World.AddImpulse(held, _firefighter.Eye.transform.forward * _config.ThrowSpeed);
        }

        private void Release()
        {
            _network.World.Release(_grip);
            _held = RopeParticle.None;
        }
    }
}
