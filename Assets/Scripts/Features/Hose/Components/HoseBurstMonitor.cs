using System;
using FlameBlame.Hose.Simulation;
using UnityEngine;

namespace FlameBlame.Hose.Components
{
    public class HoseBurstMonitor : MonoBehaviour
    {
        [SerializeField] private HoseSegment _hose;

        private BurstSequence _sequence;

        public int WeakPointIndex { get; private set; }
        public float ProgressFraction => _sequence.ProgressFraction;
        public bool HasBurst => _sequence.HasBurst;
        public bool IsTaped { get; private set; }
        public bool IsLeaking => HasBurst || IsTaped;

        public event Action Popped;

        private void Awake()
        {
            var config = _hose.Config;
            WeakPointIndex = Mathf.RoundToInt(config.BurstWeakPointFraction * (config.ParticleCount - 1));
            _sequence = new BurstSequence(config.BurstOverloadSeconds, config.BurstRecoverSeconds);
            _sequence.Popped += OnSequencePopped;
        }

        private void FixedUpdate()
        {
            _sequence.Update(_hose.PressureFractions[WeakPointIndex], Time.fixedDeltaTime);
        }

        public void Tape()
        {
            _sequence.Repair();
            IsTaped = true;
        }

        private void OnSequencePopped()
        {
            Popped?.Invoke();
        }
    }
}
