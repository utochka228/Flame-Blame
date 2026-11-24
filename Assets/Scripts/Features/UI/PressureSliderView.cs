using FlameBlame.Hose.Components;
using UnityEngine;
using UnityEngine.UI;

namespace FlameBlame.UI
{
    public class PressureSliderView : MonoBehaviour
    {
        [SerializeField] private Slider _slider;
        [SerializeField] private HoseSegment _hose;

        private void OnEnable()
        {
            _slider.onValueChanged.AddListener(OnSliderChanged);
            OnSliderChanged(_slider.value);
        }

        private void OnDisable()
        {
            _slider.onValueChanged.RemoveListener(OnSliderChanged);
        }

        private void OnSliderChanged(float value)
        {
            _hose.SetPressureFraction(value);
        }
    }
}
