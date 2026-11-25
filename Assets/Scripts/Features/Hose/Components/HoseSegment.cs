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
        private float[] _rigidityProfile;
        private float[] _rigidityCaps;
        private float[] _dampingMultipliers;
        private HeldHoseProfile _heldProfile;
        private OpenEnd[] _openEnds;

        public IReadOnlyList<Vector3> Positions => _interpolatedPositions;
        public IReadOnlyList<float> PressureFractions => _smoothedPressureFractions;
        public HoseConfigSO Config => _config;
        public XpbdRope Rope => _rope;
        public bool IsSteppedByNetwork { get; set; }
        public IReadOnlyList<OpenEnd> OpenEnds => _openEnds;

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
            _rigidityProfile = new float[_rope.Positions.Count];
            _rigidityCaps = new float[_rope.Positions.Count];
            _dampingMultipliers = new float[_rope.Positions.Count];
            _heldProfile = new HeldHoseProfile(_config.CreateHeldSettings());
            var lastIndex = _rope.Positions.Count - 1;
            _openEnds = new[]
            {
                new OpenEnd(RopeEndKind.Start, 0, 1),
                new OpenEnd(RopeEndKind.Tail, lastIndex, lastIndex - 1)
            };
            HeldHoseProfile.Clear(_rigidityCaps, _dampingMultipliers);
            Interpolate();
        }

        private void FixedUpdate()
        {
            var maxDelta = _config.PressureSmoothingFractionPerSecond * Time.fixedDeltaTime;
            for (var index = 0; index < _smoothedPressureFractions.Length; index++)
                _smoothedPressureFractions[index] = Mathf.MoveTowards(_smoothedPressureFractions[index], _targetPressureFractions[index], maxDelta);

            for (var index = 0; index < _rigidityProfile.Length; index++)
                _rigidityProfile[index] = Mathf.Min(_smoothedPressureFractions[index], _rigidityCaps[index]);

            _rope.SetRigidityProfile(_rigidityProfile);
            _rope.SetDampingProfile(_dampingMultipliers);
            if (_anchoredToOutlet)
                _rope.SetAnchor(_outlet.position, _outlet.forward);

            if (!IsSteppedByNetwork)
                _rope.Step(Time.fixedDeltaTime);
        }

        private void LateUpdate()
        {
            Interpolate();
        }

        public void SetHeld(int particleIndex)
        {
            _heldProfile.Apply(particleIndex, _rigidityCaps, _dampingMultipliers);
        }

        public void ClearHeld()
        {
            HeldHoseProfile.Clear(_rigidityCaps, _dampingMultipliers);
        }

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
