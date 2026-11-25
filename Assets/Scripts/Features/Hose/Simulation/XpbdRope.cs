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
        private readonly Vector3[] _lastFreePositions;
        private readonly Vector3[] _velocities;
        private readonly float[] _particleRigidity;
        private readonly float[] _bendAlphas;
        private readonly float[] _dampingMultipliers;
        private readonly float _floppyBendLog;
        private readonly float _rigidBendLog;

        private float _substepTime;
        private Vector3 _anchorPosition;
        private Vector3 _anchorDirection;

        public IReadOnlyList<Vector3> Positions => _positions;
        public IReadOnlyList<Vector3> PreviousPositions => _previousStepPositions;
        public bool IsPinned { get; set; } = true;

        public float RigidityFraction
        {
            set
            {
                var clamped = Mathf.Clamp01(value);
                for (var index = 0; index < _particleRigidity.Length; index++)
                    _particleRigidity[index] = clamped;
            }
        }

        private int FirstFreeIndex => IsPinned ? AnchoredCount : 0;

        public XpbdRope(RopeSettings settings, Vector3 anchorPosition, Vector3 anchorDirection, IRopeCollider collider)
        {
            _settings = settings;
            _collider = collider;
            _positions = new Vector3[settings.ParticleCount];
            _previousStepPositions = new Vector3[settings.ParticleCount];
            _substepStartPositions = new Vector3[settings.ParticleCount];
            _lastFreePositions = new Vector3[settings.ParticleCount];
            _velocities = new Vector3[settings.ParticleCount];
            _particleRigidity = new float[settings.ParticleCount];
            _bendAlphas = new float[settings.ParticleCount];
            _dampingMultipliers = new float[settings.ParticleCount];
            Array.Fill(_dampingMultipliers, 1f);
            _floppyBendLog = Mathf.Log(settings.FloppyBendCompliance);
            _rigidBendLog = Mathf.Log(settings.RigidBendCompliance);

            SetAnchor(anchorPosition, anchorDirection);
            for (var index = 0; index < _positions.Length; index++)
                _positions[index] = _anchorPosition + _anchorDirection * (index * settings.Spacing);

            Array.Copy(_positions, _previousStepPositions, _positions.Length);
            Array.Copy(_positions, _lastFreePositions, _positions.Length);
        }

        public void SetAnchor(Vector3 position, Vector3 direction)
        {
            _anchorPosition = position;
            _anchorDirection = direction.normalized;
        }

        public void SetRigidityProfile(IReadOnlyList<float> profile)
        {
            for (var index = 0; index < _particleRigidity.Length; index++)
                _particleRigidity[index] = Mathf.Clamp01(profile[index]);
        }

        public void SetDampingProfile(IReadOnlyList<float> multipliers)
        {
            for (var index = 0; index < _dampingMultipliers.Length; index++)
                _dampingMultipliers[index] = multipliers[index];
        }

        public void Step(float deltaTime)
        {
            BeginStep(deltaTime, _settings.Substeps);
            for (var substep = 0; substep < _settings.Substeps; substep++)
            {
                AdvanceSubstep();
                FinishSubstep();
            }

            EndStep();
        }

        public void BeginStep(float deltaTime, int substeps)
        {
            Array.Copy(_positions, _previousStepPositions, _positions.Length);
            if (IsPinned)
                ApplyAnchor();

            _collider.Prepare(_positions, _settings.ParticleRadius);
            _substepTime = deltaTime / substeps;
            UpdateBendAlphas(_substepTime);
        }

        public void AdvanceSubstep()
        {
            Integrate();
            SolveBends();
            SolveDistances();
        }

        public void SolveDistances()
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

        public void FinishSubstep()
        {
            CollideSubstep();
            UpdateVelocities();
        }

        public void EndStep()
        {
            RecoverStretch();
            CollideAfterStretch();

            if (!IsFinite())
                RestoreLastGoodState();
        }

        public bool IsMovable(int index) => index >= FirstFreeIndex;

        public void SetPosition(int index, Vector3 position)
        {
            _positions[index] = position;
        }

        public void AddVelocity(int index, Vector3 velocityDelta)
        {
            if (IsMovable(index))
                _velocities[index] += velocityDelta;
        }

        private void UpdateBendAlphas(float substepTime)
        {
            for (var index = 0; index < _bendAlphas.Length; index++)
                _bendAlphas[index] = Mathf.Exp(Mathf.Lerp(_floppyBendLog, _rigidBendLog, _particleRigidity[index])) / (substepTime * substepTime);
        }

        private float InverseMass(int index) => index < FirstFreeIndex ? 0f : 1f;

        private void ApplyAnchor()
        {
            _positions[0] = _anchorPosition;
            _positions[1] = _anchorPosition + _anchorDirection * _settings.Spacing;
            _velocities[0] = Vector3.zero;
            _velocities[1] = Vector3.zero;
        }

        private void Integrate()
        {
            for (var index = 0; index < _positions.Length; index++)
            {
                _substepStartPositions[index] = _positions[index];
                if (index < FirstFreeIndex) continue;

                var rigidDamping = Mathf.Lerp(1f, _settings.RigidDampingMultiplier, _particleRigidity[index]);
                var dampingFactor = Mathf.Max(0f, 1f - _settings.Damping * _dampingMultipliers[index] * rigidDamping * _substepTime);
                var velocity = (_velocities[index] + _settings.Gravity * _substepTime) * dampingFactor;
                velocity = Vector3.ClampMagnitude(velocity, _settings.MaxSpeed);
                _velocities[index] = velocity;
                _positions[index] += velocity * _substepTime;
            }
        }

        private void SolveBends()
        {
            for (var index = AnchoredCount; index < _positions.Length; index++)
            {
                var straightTarget = 2f * _positions[index - 1] - _positions[index - 2];
                var correction = (straightTarget - _positions[index]) / (1f + _bendAlphas[index]);
                _positions[index] += correction;
            }
        }

        private void CollideSubstep()
        {
            CollideParticles(_substepStartPositions, _settings.GroundFriction);
            CollideSegments();
        }

        private void CollideAfterStretch()
        {
            CollideParticles(_previousStepPositions, 0f);
        }

        private void CollideParticles(Vector3[] startPositions, float friction)
        {
            for (var index = FirstFreeIndex; index < _positions.Length; index++)
            {
                var unresolved = _collider.Sweep(startPositions[index], _positions[index], _settings.ParticleRadius);
                _positions[index] = unresolved;
                var resolved = _collider.Resolve(unresolved, _lastFreePositions[index], _settings.ParticleRadius);
                var push = resolved - unresolved;
                if (push.magnitude <= _settings.ParticleRadius)
                    _lastFreePositions[index] = resolved;

                if (push.sqrMagnitude < MinLength * MinLength) continue;

                var normal = push.normalized;
                var motion = resolved - startPositions[index];
                var tangentialMotion = motion - normal * Vector3.Dot(motion, normal);
                _positions[index] = resolved - tangentialMotion * friction;
            }
        }

        private void CollideSegments()
        {
            for (var index = Mathf.Max(FirstFreeIndex - 1, 0); index < _positions.Length - 1; index++)
            {
                var weightSum = InverseMass(index) + InverseMass(index + 1);
                if (weightSum <= 0f) continue;

                var midpoint = (_positions[index] + _positions[index + 1]) * 0.5f;
                var reference = (_lastFreePositions[index] + _lastFreePositions[index + 1]) * 0.5f;
                var push = _collider.Resolve(midpoint, reference, _settings.ParticleRadius) - midpoint;
                if (push.sqrMagnitude < MinLength * MinLength) continue;

                _positions[index] += push * (2f * InverseMass(index) / weightSum);
                _positions[index + 1] += push * (2f * InverseMass(index + 1) / weightSum);
            }
        }

        private void UpdateVelocities()
        {
            for (var index = FirstFreeIndex; index < _positions.Length; index++)
                _velocities[index] = (_positions[index] - _substepStartPositions[index]) / _substepTime;
        }

        private void RecoverStretch()
        {
            var maxLength = _settings.Spacing * (1f + _settings.MaxStretchFraction);
            for (var index = Mathf.Max(FirstFreeIndex, 1); index < _positions.Length; index++)
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
