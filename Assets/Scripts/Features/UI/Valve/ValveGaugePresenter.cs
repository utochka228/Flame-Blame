using FlameBlame.Core.Events;
using FlameBlame.Firefighter.Events;
using FlameBlame.Pressure.Components;
using UnityEngine;
using UnityEngine.UIElements;

namespace FlameBlame.UI.Valve
{
    [RequireComponent(typeof(UIDocument))]
    public class ValveGaugePresenter : MonoBehaviour
    {
        [SerializeField] private PressureSource _source;

        private ValveGaugeView _view;

        private void OnEnable()
        {
            _view = new ValveGaugeView(GetComponent<UIDocument>().rootVisualElement, _source.PressureFraction, _source.IsOpen);
            _source.Changed += OnSourceChanged;
            EventBus.Subscribe<ValveAdjusted>(OnValveAdjusted);
        }

        private void OnDisable()
        {
            _source.Changed -= OnSourceChanged;
            EventBus.Unsubscribe<ValveAdjusted>(OnValveAdjusted);
            _view.Dispose();
        }

        private void OnSourceChanged()
        {
            _view.Show(_source.PressureFraction, _source.IsOpen);
        }

        private void OnValveAdjusted(ValveAdjusted adjusted)
        {
            if (adjusted.IsAtLimit && adjusted.Valve == _source)
                _view.ShowLimit();
        }
    }
}
