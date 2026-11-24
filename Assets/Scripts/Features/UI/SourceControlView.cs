using FlameBlame.Pressure.Components;
using UnityEngine;
using UnityEngine.UI;

namespace FlameBlame.UI
{
    public class SourceControlView : MonoBehaviour
    {
        [SerializeField] private Slider _pressureSlider;
        [SerializeField] private Toggle _valveToggle;
        [SerializeField] private PressureSource _source;

        private void OnEnable()
        {
            _pressureSlider.onValueChanged.AddListener(OnPressureChanged);
            _valveToggle.onValueChanged.AddListener(OnValveChanged);
        }

        private void Start()
        {
            OnPressureChanged(_pressureSlider.value);
            OnValveChanged(_valveToggle.isOn);
        }

        private void OnDisable()
        {
            _pressureSlider.onValueChanged.RemoveListener(OnPressureChanged);
            _valveToggle.onValueChanged.RemoveListener(OnValveChanged);
        }

        private void OnPressureChanged(float value)
        {
            _source.SetPressureFraction(value);
        }

        private void OnValveChanged(bool isOpen)
        {
            _source.IsOpen = isOpen;
        }
    }
}
