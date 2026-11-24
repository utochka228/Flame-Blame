using System.Collections.Generic;
using FlameBlame.Hose.Config;
using FlameBlame.Hose.Simulation;
using UnityEngine;

namespace FlameBlame.Hose.Components
{
    public class HoseSegment : MonoBehaviour
    {
        [SerializeField] private HoseConfigSO _config;
        [SerializeField] private Transform _outlet;

        private XpbdRope _rope;
        private Vector3[] _interpolatedPositions;
        private float _targetPressureFraction;
        private float _smoothedPressureFraction;

        public IReadOnlyList<Vector3> Positions => _interpolatedPositions;
        public float PressureFraction => _smoothedPressureFraction;
        public HoseConfigSO Config => _config;

        public void SetPressureFraction(float pressureFraction)
        {
            _targetPressureFraction = Mathf.Clamp01(pressureFraction);
        }

        private void Awake()
        {
            _rope = new XpbdRope(_config.CreateRopeSettings(), _outlet.position, _outlet.forward,
                new PhysicsRopeCollider(_config.CollisionLayers, transform));
            _interpolatedPositions = new Vector3[_rope.Positions.Count];
            Interpolate();
        }

        private void FixedUpdate()
        {
            _smoothedPressureFraction = Mathf.MoveTowards(_smoothedPressureFraction, _targetPressureFraction,
                _config.PressureSmoothingFractionPerSecond * Time.fixedDeltaTime);
            _rope.RigidityFraction = _smoothedPressureFraction;
            _rope.SetAnchor(_outlet.position, _outlet.forward);
            _rope.Step(Time.fixedDeltaTime);
        }

        private void LateUpdate()
        {
            Interpolate();
        }

        private void Interpolate()
        {
            var alpha = Mathf.Clamp01((Time.time - Time.fixedTime) / Time.fixedDeltaTime);
            for (var index = 0; index < _interpolatedPositions.Length; index++)
                _interpolatedPositions[index] = Vector3.Lerp(_rope.PreviousPositions[index], _rope.Positions[index], alpha);
        }
    }
}
