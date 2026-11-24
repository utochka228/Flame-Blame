using UnityEngine;

namespace FlameBlame.Pressure.Components
{
    public class PressureSource : MonoBehaviour
    {
        [SerializeField] [Min(0f)] private float _maxPressure = 200f;

        private float _pressureFraction;

        public bool IsOpen { get; set; } = true;
        public float CurrentPressure => IsOpen ? _pressureFraction * _maxPressure : 0f;

        public void SetPressureFraction(float pressureFraction)
        {
            _pressureFraction = Mathf.Clamp01(pressureFraction);
        }
    }
}
