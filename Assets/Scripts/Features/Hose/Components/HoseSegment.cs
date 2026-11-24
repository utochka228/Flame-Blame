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
        [SerializeField] private bool _anchoredToOutlet = true;

        private XpbdRope _rope;
        private Vector3[] _interpolatedPositions;
        private float[] _targetPressureFractions;
        private float[] _smoothedPressureFractions;

        public IReadOnlyList<Vector3> Positions => _interpolatedPositions;
        public IReadOnlyList<float> PressureFractions => _smoothedPressureFractions;
        public float InletPressureFraction => _smoothedPressureFractions[0];
        public HoseConfigSO Config => _config;
        public XpbdRope Rope => _rope;
        public bool IsSteppedByNetwork { get; set; }

        public void SetPressureFraction(float pressureFraction)
        {
            for (var index = 0; index < _targetPressureFractions.Length; index++)
                SetTarget(index, pressureFraction);
        }

        public void SetPressureProfile(IReadOnlyList<float> pressureFractions)
        {
            for (var index = 0; index < _targetPressureFractions.Length; index++)
                SetTarget(index, pressureFractions[index]);
        }

        private void Awake()
        {
            _rope = new XpbdRope(_config.CreateRopeSettings(), _outlet.position, _outlet.forward,
                new PhysicsRopeCollider(_config.CollisionLayers, transform))
            {
                IsPinned = _anchoredToOutlet
            };
            _interpolatedPositions = new Vector3[_rope.Positions.Count];
            _targetPressureFractions = new float[_rope.Positions.Count];
            _smoothedPressureFractions = new float[_rope.Positions.Count];
            Interpolate();
        }

        private void FixedUpdate()
        {
            var maxDelta = _config.PressureSmoothingFractionPerSecond * Time.fixedDeltaTime;
            for (var index = 0; index < _smoothedPressureFractions.Length; index++)
                _smoothedPressureFractions[index] = Mathf.MoveTowards(_smoothedPressureFractions[index], _targetPressureFractions[index], maxDelta);

            _rope.SetRigidityProfile(_smoothedPressureFractions);
            if (_anchoredToOutlet)
                _rope.SetAnchor(_outlet.position, _outlet.forward);

            if (!IsSteppedByNetwork)
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

        private void SetTarget(int index, float pressureFraction)
        {
            _targetPressureFractions[index] = Mathf.Max(0f, pressureFraction);
        }
    }
}
