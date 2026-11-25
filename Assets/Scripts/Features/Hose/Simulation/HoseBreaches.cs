using System;
using System.Collections.Generic;

namespace FlameBlame.Hose.Simulation
{
    public class HoseBreaches
    {
        private readonly BreachSettings _settings;
        private readonly BurstSequence _sequence;
        private readonly List<Breach> _breaches = new List<Breach>();
        private readonly BurnTimer[] _burnTimers;

        public HoseBreaches(BreachSettings settings)
        {
            _settings = settings;
            _burnTimers = new BurnTimer[settings.ParticleCount];
            for (var index = 0; index < _burnTimers.Length; index++)
                _burnTimers[index] = new BurnTimer(settings.BurnSeconds);

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
            for (var index = 0; index < _burnTimers.Length; index++)
            {
                if (_burnTimers[index].Step(heat[index] >= _settings.HeatThreshold, deltaTime))
                    Burn(index);
            }
        }

        public bool Tape(int index)
        {
            var found = IndexContaining(index);
            if (found < 0 || _breaches[found].IsTaped) return false;

            var breach = _breaches[found];
            _breaches[found] = new Breach(breach.FirstIndex, breach.LastIndex, _settings.TapeLeakLoss, true);
            if (Covers(breach, _settings.WeakPointIndex))
                _sequence.Repair();

            return true;
        }

        private void Burn(int index)
        {
            var existing = IndexContaining(index);
            if (existing >= 0 && !_breaches[existing].IsTaped) return;

            if (Open(index))
                Burned?.Invoke(index);
        }

        private void OnSequencePopped()
        {
            Open(_settings.WeakPointIndex);
            Popped?.Invoke();
        }

        private bool Open(int index)
        {
            var containing = IndexContaining(index);
            if (containing >= 0)
            {
                _breaches[containing] = Reopened(_breaches[containing]);
                MergeOpenNeighbours();
                return true;
            }

            var neighbour = FindOpenNeighbour(index);
            if (neighbour >= 0)
            {
                _breaches[neighbour] = Grown(_breaches[neighbour], index);
                MergeOpenNeighbours();
                return false;
            }

            _breaches.Add(new Breach(index, index, _settings.BurstLeakLoss, false));
            MergeOpenNeighbours();
            return true;
        }

        private Breach Reopened(Breach breach) => new Breach(breach.FirstIndex, breach.LastIndex, _settings.BurstLeakLoss, false);

        private Breach Grown(Breach breach, int index) =>
            new Breach(Math.Min(breach.FirstIndex, index), Math.Max(breach.LastIndex, index), _settings.BurstLeakLoss, false);

        private void MergeOpenNeighbours()
        {
            for (var first = 0; first < _breaches.Count; first++)
            {
                for (var second = first + 1; second < _breaches.Count; second++)
                {
                    if (!AreOpenNeighbours(_breaches[first], _breaches[second])) continue;

                    _breaches[first] = Merged(_breaches[first], _breaches[second]);
                    _breaches.RemoveAt(second);
                    second = first;
                }
            }
        }

        private static bool AreOpenNeighbours(Breach a, Breach b) =>
            !a.IsTaped && !b.IsTaped && a.LastIndex + 1 >= b.FirstIndex && b.LastIndex + 1 >= a.FirstIndex;

        private Breach Merged(Breach a, Breach b) =>
            new Breach(Math.Min(a.FirstIndex, b.FirstIndex), Math.Max(a.LastIndex, b.LastIndex), _settings.BurstLeakLoss, false);

        private int FindOpenNeighbour(int index)
        {
            for (var found = 0; found < _breaches.Count; found++)
            {
                var breach = _breaches[found];
                if (!breach.IsTaped && (breach.LastIndex + 1 == index || breach.FirstIndex - 1 == index))
                    return found;
            }

            return -1;
        }

        private int IndexContaining(int particleIndex)
        {
            for (var index = 0; index < _breaches.Count; index++)
            {
                if (Covers(_breaches[index], particleIndex))
                    return index;
            }

            return -1;
        }

        private static bool Covers(Breach breach, int particleIndex) => particleIndex >= breach.FirstIndex && particleIndex <= breach.LastIndex;
    }
}
