using System;
using System.Collections.Generic;
using FlameBlame.Core.Extensions;
using UnityEngine;

namespace FlameBlame.Hose.Simulation
{
    public class XpbdRope
    {
        private const int AnchoredCount = 2;
        private const float MinLength = 1e-6f;

        private readonly RopeSettings _settings;
        private readonly IRopeCollider _collider;
        private readonly Vector3[] _positions;
        private readonly Vector3[] _previousStepPositions;
        private readonly Vector3[] _substepStartPositions;
        private readonly Vector3[] _velocities;

        private Vector3 _anchorPosition;
        private Vector3 _anchorDirection;
        private float _rigidityFraction;

        public IReadOnlyList<Vector3> Positions => _positions;
        public IReadOnlyList<Vector3> PreviousPositions => _previousStepPositions;

        public float RigidityFraction
        {
            get => _rigidityFraction;
            set => _rigidityFraction = Mathf.Clamp01(value);
        }

        public XpbdRope(RopeSettings settings, Vector3 anchorPosition, Vector3 anchorDirection, IRopeCollider collider)
        {
            _settings = settings;
            _collider = collider;
            _positions = new Vector3[settings.ParticleCount];
            _previousStepPositions = new Vector3[settings.ParticleCount];
            _substepStartPositions = new Vector3[settings.ParticleCount];
            _velocities = new Vector3[settings.ParticleCount];

            SetAnchor(anchorPosition, anchorDirection);
            for (var index = 0; index < _positions.Length; index++)
                _positions[index] = _anchorPosition + _anchorDirection * (index * settings.Spacing);

            Array.Copy(_positions, _previousStepPositions, _positions.Length);
        }

        public void SetAnchor(Vector3 position, Vector3 direction)
        {
            _anchorPosition = position;
            _anchorDirection = direction.normalized;
        }

        public void Step(float deltaTime)
        {
            Array.Copy(_positions, _previousStepPositions, _positions.Length);
            ApplyAnchor();
            _collider.Prepare(_positions, _settings.ParticleRadius);

            var substepTime = deltaTime / _settings.Substeps;
            var bendAlpha = BendCompliance() / (substepTime * substepTime);
            for (var substep = 0; substep < _settings.Substeps; substep++)
            {
                Integrate(substepTime);
                SolveBends(bendAlpha);
                SolveDistances();

                Collide();
                UpdateVelocities(substepTime);
            }

            RecoverStretch();

            if (!IsFinite())
                RestoreLastGoodState();
        }

        private float BendCompliance()
        {
            var floppyLog = Mathf.Log(_settings.FloppyBendCompliance);
            var rigidLog = Mathf.Log(_settings.RigidBendCompliance);
            return Mathf.Exp(Mathf.Lerp(floppyLog, rigidLog, _rigidityFraction));
        }

        private static float InverseMass(int index) => index < AnchoredCount ? 0f : 1f;

        private void ApplyAnchor()
        {
            _positions[0] = _anchorPosition;
            _positions[1] = _anchorPosition + _anchorDirection * _settings.Spacing;
            _velocities[0] = Vector3.zero;
            _velocities[1] = Vector3.zero;
        }

        private void Integrate(float substepTime)
        {
            var dampingFactor = Mathf.Max(0f, 1f - _settings.Damping * substepTime);
            for (var index = 0; index < _positions.Length; index++)
            {
                _substepStartPositions[index] = _positions[index];
                if (index < AnchoredCount) continue;

                var velocity = (_velocities[index] + _settings.Gravity * substepTime) * dampingFactor;
                velocity = Vector3.ClampMagnitude(velocity, _settings.MaxSpeed);
                _velocities[index] = velocity;
                _positions[index] += velocity * substepTime;
            }
        }

        private void SolveDistances()
        {
            for (var index = 0; index < _positions.Length - 1; index++)
            {
                var weightSum = InverseMass(index) + InverseMass(index + 1);
                if (weightSum <= 0f) continue;

                var delta = _positions[index + 1] - _positions[index];
                var length = delta.magnitude;
                if (length < MinLength) continue;

                var correction = delta / length * ((length - _settings.Spacing) / weightSum);
                _positions[index] += correction * InverseMass(index);
                _positions[index + 1] -= correction * InverseMass(index + 1);
            }
        }

        private void SolveBends(float scaledCompliance)
        {
            for (var index = AnchoredCount; index < _positions.Length; index++)
            {
                var straightTarget = 2f * _positions[index - 1] - _positions[index - 2];
                var correction = (straightTarget - _positions[index]) / (1f + scaledCompliance);
                _positions[index] += correction;
            }
        }

        private void Collide()
        {
            for (var index = AnchoredCount; index < _positions.Length; index++)
            {
                var unresolved = _positions[index];
                var resolved = _collider.Resolve(unresolved, _settings.ParticleRadius);
                var push = resolved - unresolved;
                if (push.sqrMagnitude < MinLength * MinLength) continue;

                var normal = push.normalized;
                var motion = resolved - _substepStartPositions[index];
                var tangentialMotion = motion - normal * Vector3.Dot(motion, normal);
                _positions[index] = resolved - tangentialMotion * _settings.GroundFriction;
            }
        }

        private void UpdateVelocities(float substepTime)
        {
            for (var index = AnchoredCount; index < _positions.Length; index++)
                _velocities[index] = (_positions[index] - _substepStartPositions[index]) / substepTime;
        }

        private void RecoverStretch()
        {
            var maxLength = _settings.Spacing * (1f + _settings.MaxStretchFraction);
            for (var index = AnchoredCount; index < _positions.Length; index++)
            {
                var delta = _positions[index] - _positions[index - 1];
                var length = delta.magnitude;
                if (length <= maxLength) continue;

                _positions[index] = _positions[index - 1] + delta / length * _settings.Spacing;
                _velocities[index] = Vector3.zero;
            }
        }

        private bool IsFinite()
        {
            foreach (var position in _positions)
            {
                if (!position.IsFinite()) return false;
            }

            return true;
        }

        private void RestoreLastGoodState()
        {
            Array.Copy(_previousStepPositions, _positions, _positions.Length);
            Array.Clear(_velocities, 0, _velocities.Length);
        }
    }
}
