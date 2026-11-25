using System;
using System.Collections.Generic;
using UnityEngine;

namespace FlameBlame.Hose.Simulation
{
    public class HoseBreaches
    {
        private readonly BreachSettings _settings;
        private readonly BurstSequence _sequence;
        private readonly List<Breach> _breaches = new List<Breach>();
        private readonly float[] _burnProgress;

        public HoseBreaches(BreachSettings settings)
        {
            _settings = settings;
            _burnProgress = new float[settings.ParticleCount];
            _sequence = new BurstSequence(settings.OverloadSeconds, settings.RecoverSeconds);
            _sequence.Popped += OnSequencePopped;
        }

        public IReadOnlyList<Breach> Breaches => _breaches;
        public float WeakPointProgressFraction => _sequence.ProgressFraction;

        public event Action Popped;
        public event Action<int> Burned;

        public void Step(float weakPointPressureFraction, IReadOnlyList<float> heat, float deltaTime)
        {
            _sequence.Update(weakPointPressureFraction, deltaTime);
            for (var index = 0; index < _burnProgress.Length; index++)
                StepBurn(index, heat[index] >= _settings.HeatThreshold, deltaTime);
        }

        public bool Tape(int index)
        {
            var existing = IndexOf(index);
            if (existing < 0 || _breaches[existing].IsTaped) return false;

            _breaches[existing] = new Breach(index, _settings.TapeLeakLoss, true);
            if (index == _settings.WeakPointIndex)
                _sequence.Repair();

            return true;
        }

        private void StepBurn(int index, bool isHeated, float deltaTime)
        {
            var direction = isHeated ? 1f : -1f;
            _burnProgress[index] = Mathf.Clamp01(_burnProgress[index] + direction * deltaTime / _settings.BurnSeconds);
            if (_burnProgress[index] < 1f) return;

            _burnProgress[index] = 0f;
            var existing = IndexOf(index);
            if (existing >= 0 && !_breaches[existing].IsTaped) return;

            Open(index);
            Burned?.Invoke(index);
        }

        private void OnSequencePopped()
        {
            Open(_settings.WeakPointIndex);
            Popped?.Invoke();
        }

        private void Open(int index)
        {
            var breach = new Breach(index, _settings.BurstLeakLoss, false);
            var existing = IndexOf(index);
            if (existing < 0)
                _breaches.Add(breach);
            else
                _breaches[existing] = breach;
        }

        private int IndexOf(int particleIndex)
        {
            for (var index = 0; index < _breaches.Count; index++)
            {
                if (_breaches[index].Index == particleIndex)
                    return index;
            }

            return -1;
        }
    }
}
