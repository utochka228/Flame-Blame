using System;
using System.Collections.Generic;
using FlameBlame.Hose.Simulation;
using UnityEngine;

namespace FlameBlame.Hose.Components
{
    public class HoseBurstMonitor : MonoBehaviour
    {
        [SerializeField] private HoseSegment _hose;

        private HoseBreaches _breaches;
        private float[] _heat;

        public int WeakPointIndex { get; private set; }
        public float ProgressFraction => _breaches.WeakPointProgressFraction;
        public IReadOnlyList<Breach> Breaches => _breaches.Breaches;
        public bool IsLeaking => Breaches.Count > 0;

        public event Action Popped;

        private void Awake()
        {
            var settings = _hose.Config.CreateBreachSettings();
            WeakPointIndex = settings.WeakPointIndex;
            _heat = new float[settings.ParticleCount];
            _breaches = new HoseBreaches(settings);
        }

        private void OnEnable()
        {
            _breaches.Popped += OnPopped;
        }

        private void OnDisable()
        {
            _breaches.Popped -= OnPopped;
        }

        private void FixedUpdate()
        {
            _breaches.Step(_hose.PressureFractions[WeakPointIndex], _heat, Time.fixedDeltaTime);
        }

        public bool Tape(int index) => _breaches.Tape(index);

        private void OnPopped()
        {
            Popped?.Invoke();
        }
    }
}
