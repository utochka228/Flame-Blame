using System;
using UnityEngine;

namespace FlameBlame.Pressure.Components
{
    public class PressureSource : MonoBehaviour
    {
        [SerializeField] [Min(0f)] private float _maxPressure = 200f;

        [SerializeField] [Range(0f, 1f)] private float _pressureFraction;

        private bool _isOpen = true;

        public bool IsOpen
        {
            get => _isOpen;
            set
            {
                if (value == _isOpen) return;

                _isOpen = value;
                Changed?.Invoke();
            }
        }

        public float PressureFraction => _pressureFraction;
        public float CurrentPressure => IsOpen ? _pressureFraction * _maxPressure : 0f;

        public event Action Changed;

        public void SetPressureFraction(float pressureFraction)
        {
            var clamped = Mathf.Clamp01(pressureFraction);
            if (Mathf.Approximately(clamped, _pressureFraction)) return;

            _pressureFraction = clamped;
            Changed?.Invoke();
        }
    }
}
