using System;
using System.Collections.Generic;
using FlameBlame.Core.Extensions;
using Unity.Profiling;
using UnityEngine;

namespace FlameBlame.Hose.Simulation
{
    public class XpbdRope
    {
        private const int AnchoredCount = 2;
        private const float MinLength = 1e-6f;

        private static readonly ProfilerMarker IntegrateMarker = new ProfilerMarker("XpbdRope.Integrate");
        private static readonly ProfilerMarker BendsMarker = new ProfilerMarker("XpbdRope.SolveBends");
        private static readonly ProfilerMarker DistancesMarker = new ProfilerMarker("XpbdRope.SolveDistances");
        private static readonly ProfilerMarker CollideParticlesMarker = new ProfilerMarker("XpbdRope.CollideParticles");
        private static readonly ProfilerMarker CollideSegmentsMarker = new ProfilerMarker("XpbdRope.CollideSegments");
        private static readonly ProfilerMarker EndStepMarker = new ProfilerMarker("XpbdRope.EndStep");

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
        private readonly bool[] _frozen;
        private readonly float[] _slipSecondsLeft;
        private readonly bool[] _clamped;
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
            _frozen = new bool[settings.ParticleCount];
            _slipSecondsLeft = new float[settings.ParticleCount];
            _clamped = new bool[settings.ParticleCount];
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

        public void SetFrozenProfile(IReadOnlyList<bool> frozen)
        {
            for (var index = 0; index < _frozen.Length; index++)
                _frozen[index] = frozen[index] && _slipSecondsLeft[index] <= 0f;
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
            for (var index = 0; index < _slipSecondsLeft.Length; index++)
                _slipSecondsLeft[index] = Mathf.Max(0f, _slipSecondsLeft[index] - deltaTime);
        }

        public void AdvanceSubstep()
        {
            Integrate();
            SolveBends();
            SolveDistances();
        }

        public void SolveDistances()
        {
            using var marker = DistancesMarker.Auto();
            for (var index = 0; index < _positions.Length - 1; index++)
            {
                var weight = InverseMass(index);
                var nextWeight = InverseMass(index + 1);
                var weightSum = weight + nextWeight;
                if (weightSum <= 0f) continue;

                var delta = _positions[index + 1] - _positions[index];
                var length = delta.magnitude;
                if (length < MinLength) continue;

                var correction = delta / length * ((length - _settings.Spacing) / weightSum);
                _positions[index] += correction * weight;
                _positions[index + 1] -= correction * nextWeight;
            }
        }

        public void FinishSubstep()
        {
            CollideSubstep();
            UpdateVelocities();
        }

        public void EndStep()
        {
            using var marker = EndStepMarker.Auto();
            RecoverStretch();
            ReleaseFreezesPulledTaut();
            CollideAfterStretch();

            if (!IsFinite())
                RestoreLastGoodState();
        }

        public bool IsMovable(int index) => index >= FirstFreeIndex && !_frozen[index];

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

        private float InverseMass(int index) => IsMovable(index) ? 1f : 0f;

        private void ApplyAnchor()
        {
            _positions[0] = _anchorPosition;
            _positions[1] = _anchorPosition + _anchorDirection * _settings.Spacing;
            _velocities[0] = Vector3.zero;
            _velocities[1] = Vector3.zero;
        }

        private void Integrate()
        {
            using var marker = IntegrateMarker.Auto();
            for (var index = 0; index < _positions.Length; index++)
            {
                _substepStartPositions[index] = _positions[index];
                if (_frozen[index])
                    _velocities[index] = Vector3.zero;

                if (!IsMovable(index)) continue;

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
            using var marker = BendsMarker.Auto();
            var outerWeight = InverseMass(AnchoredCount - 2);
            var middleWeight = InverseMass(AnchoredCount - 1);
            for (var index = AnchoredCount; index < _positions.Length; index++)
            {
                var innerWeight = InverseMass(index);
                var weightSum = outerWeight + 4f * middleWeight + innerWeight;
                if (weightSum > 0f)
                {
                    var curvature = _positions[index - 2] - 2f * _positions[index - 1] + _positions[index];
                    var correction = -curvature / (weightSum + _bendAlphas[index]);
                    _positions[index - 2] += correction * outerWeight;
                    _positions[index - 1] -= correction * (2f * middleWeight);
                    _positions[index] += correction * innerWeight;
                }

                outerWeight = middleWeight;
                middleWeight = innerWeight;
            }
        }

        private void CollideSubstep()
        {
            CollideParticles(_substepStartPositions, _settings.GroundFriction);
        }

        private void CollideAfterStretch()
        {
            CollideParticles(_previousStepPositions, 0f);
            CollideSegments();
        }

        private void CollideParticles(Vector3[] startPositions, float friction)
        {
            using var marker = CollideParticlesMarker.Auto();
            for (var index = FirstFreeIndex; index < _positions.Length; index++)
            {
                if (_frozen[index]) continue;

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
            using var marker = CollideSegmentsMarker.Auto();
            for (var index = Mathf.Max(FirstFreeIndex - 1, 0); index < _positions.Length - 1; index++)
            {
                var weight = InverseMass(index);
                var nextWeight = InverseMass(index + 1);
                var weightSum = weight + nextWeight;
                if (weightSum <= 0f) continue;

                var midpoint = (_positions[index] + _positions[index + 1]) * 0.5f;
                var reference = (_lastFreePositions[index] + _lastFreePositions[index + 1]) * 0.5f;
                var push = _collider.Resolve(midpoint, reference, _settings.ParticleRadius) - midpoint;
                if (push.sqrMagnitude < MinLength * MinLength) continue;

                _positions[index] += push * (2f * weight / weightSum);
                _positions[index + 1] += push * (2f * nextWeight / weightSum);
            }
        }

        private void UpdateVelocities()
        {
            for (var index = FirstFreeIndex; index < _positions.Length; index++)
            {
                if (IsMovable(index))
                    _velocities[index] = (_positions[index] - _substepStartPositions[index]) / _substepTime;
            }
        }

        private void ReleaseFreezesPulledTaut()
        {
            for (var index = 0; index < _positions.Length; index++)
            {
                if (!_frozen[index] || !IsRunClamped(index, -1) && !IsRunClamped(index, 1)) continue;

                _frozen[index] = false;
                _slipSecondsLeft[index] = _settings.SlipSeconds;
            }
        }

        private bool IsRunClamped(int frozenIndex, int direction)
        {
            for (var index = frozenIndex + direction; index >= 0 && index < _positions.Length && IsMovable(index); index += direction)
            {
                if (_clamped[index]) return true;
            }

            return false;
        }

        private void RecoverStretch()
        {
            var maxLength = _settings.Spacing * (1f + _settings.MaxStretchFraction);
            Array.Clear(_clamped, 0, _clamped.Length);
            for (var index = Mathf.Max(FirstFreeIndex, 1); index < _positions.Length; index++)
            {
                if (_frozen[index]) continue;

                var delta = _positions[index] - _positions[index - 1];
                var length = delta.magnitude;
                if (length <= maxLength) continue;

                _positions[index] = _positions[index - 1] + delta / length * _settings.Spacing;
                _velocities[index] = Vector3.zero;
                _clamped[index] = true;
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
