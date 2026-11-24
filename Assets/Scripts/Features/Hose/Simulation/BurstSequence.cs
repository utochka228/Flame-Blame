using System;
using UnityEngine;

namespace FlameBlame.Hose.Simulation
{
    public class BurstSequence
    {
        private readonly float _overloadSeconds;
        private readonly float _recoverSeconds;

        public bool HasBurst { get; private set; }
        public float ProgressFraction { get; private set; }

        public event Action Popped;

        public BurstSequence(float overloadSeconds, float recoverSeconds)
        {
            _overloadSeconds = overloadSeconds;
            _recoverSeconds = recoverSeconds;
        }

        public void Repair()
        {
            HasBurst = false;
            ProgressFraction = 0f;
        }

        public void Update(float pressureFraction, float deltaTime)
        {
            if (HasBurst) return;

            var rate = pressureFraction > 1f ? 1f / _overloadSeconds : -1f / _recoverSeconds;
            ProgressFraction = Mathf.Clamp01(ProgressFraction + rate * deltaTime);
            if (ProgressFraction < 1f) return;

            HasBurst = true;
            ProgressFraction = 0f;
            Popped?.Invoke();
        }
    }
}
