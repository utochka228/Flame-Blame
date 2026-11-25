using System;
using System.Collections.Generic;
using FlameBlame.Hose.Simulation;
using UnityEngine;

namespace FlameBlame.Hose.Components
{
    public class HoseBurstMonitor : MonoBehaviour
    {
        [SerializeField] private HoseSegment _hose;
        [SerializeField] private HeatField _heatField;

        private HoseBreaches _breaches;
        private BreachSettings _settings;
        private float[] _heat;
        private float _secondsSinceHeatCheck;

        public int WeakPointIndex { get; private set; }
        public float ProgressFraction => _breaches.WeakPointProgressFraction;
        public IReadOnlyList<Breach> Breaches => _breaches.Breaches;
        public bool IsLeaking => Breaches.Count > 0;

        public event Action Popped;

        private void Awake()
        {
            _settings = _hose.Config.CreateBreachSettings();
            WeakPointIndex = _settings.WeakPointIndex;
            _heat = new float[_settings.ParticleCount];
            _breaches = new HoseBreaches(_settings);
        }

        private void OnEnable()
        {
            _breaches.Popped += OnPopped;
            _hose.Config.Changed += OnConfigChanged;
        }

        private void OnDisable()
        {
            _breaches.Popped -= OnPopped;
            _hose.Config.Changed -= OnConfigChanged;
        }

        private void FixedUpdate()
        {
            SampleHeat();
            _breaches.Step(_hose.PressureFractions[WeakPointIndex], _heat, Time.fixedDeltaTime);
        }

        private void SampleHeat()
        {
            if (!_heatField) return;

            _secondsSinceHeatCheck += Time.fixedDeltaTime;
            if (_secondsSinceHeatCheck < 1f / _hose.Config.Heat.ChecksPerSecond) return;

            _secondsSinceHeatCheck = 0f;
            var positions = _hose.Rope.Positions;
            for (var index = 0; index < _heat.Length; index++)
                _heat[index] = _heatField.Sample(positions[index]);
        }

        public bool Tape(int index) => _breaches.Tape(index);

        private void OnConfigChanged()
        {
            _hose.Config.ApplyTo(_settings);
            WeakPointIndex = _settings.WeakPointIndex;
        }

        private void OnPopped()
        {
            Popped?.Invoke();
        }
    }
}
