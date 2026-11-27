using System;
using DG.Tweening;
using FlameBlame.Core.Extensions;
using FlameBlame.UI.Kit;
using UnityEngine;
using UnityEngine.UIElements;

namespace FlameBlame.UI.Valve
{
    public class ValveGaugeView : IDisposable
    {
        private const string ClosedClass = "valve-gauge--closed";

        private readonly VisualElement _gauge;
        private readonly VisualElement _track;
        private readonly VisualElement _fill;
        private readonly Label _state;
        private readonly Label _percent;
        private float _shownFraction;
        private bool _isShownOpen;
        private Tween _fillTween;
        private Tween _percentPop;
        private Tween _stateSquash;
        private Tween _trackSquash;
        private Tween _gaugeShake;

        public ValveGaugeView(VisualElement root, float pressureFraction, bool isOpen)
        {
            _gauge = root.Q("valveGauge");
            _track = root.Q(className: "valve-gauge__track");
            _fill = root.Q("valveFill");
            _state = root.Q<Label>("valveState");
            _percent = root.Q<Label>("valvePercent");
            _shownFraction = pressureFraction;
            _isShownOpen = isOpen;
            SetFill(pressureFraction);
            SetState(isOpen);
        }

        public void Show(float pressureFraction, bool isOpen)
        {
            if (!Mathf.Approximately(pressureFraction, _shownFraction))
                ShowPressure(pressureFraction);
            if (isOpen != _isShownOpen)
                ShowValve(isOpen);
        }

        public void ShowLimit()
        {
            UiTween.Restart(ref _gaugeShake, UiTween.Shake(_gauge, 10f, 0.25f));
            UiTween.Restart(ref _percentPop, UiTween.Squash(_percent, new Vector2(1.25f, 0.8f), 0.3f));
        }

        public void Dispose()
        {
            _fillTween?.Kill();
            _percentPop?.Kill();
            _stateSquash?.Kill();
            _trackSquash?.Kill();
            _gaugeShake?.Kill();
        }

        private void ShowPressure(float pressureFraction)
        {
            var shown = _shownFraction;
            var isRising = pressureFraction > shown;
            _shownFraction = pressureFraction;
            UiTween.Restart(ref _fillTween, DOTween.To(() => shown, value =>
                {
                    shown = value;
                    SetFill(value);
                }, pressureFraction, 0.2f)
                .SetEase(Ease.OutCubic)
                .SetUpdate(true));
            UiTween.Restart(ref _percentPop, UiTween.Pop(_percent, isRising ? 1.15f : 0.85f, 0.25f));
            UiTween.Restart(ref _trackSquash, UiTween.Squash(_track, new Vector2(1f, isRising ? 1.3f : 0.75f), 0.25f));
        }

        private void ShowValve(bool isOpen)
        {
            _isShownOpen = isOpen;
            SetState(isOpen);
            UiTween.Restart(ref _stateSquash, UiTween.Squash(_state, new Vector2(1.3f, 0.7f), 0.35f));
            UiTween.Restart(ref _trackSquash, UiTween.Squash(_track, new Vector2(1.05f, isOpen ? 1.5f : 0.5f), 0.4f));
            UiTween.Restart(ref _gaugeShake, UiTween.Shake(_gauge, 6f, 0.3f));
        }

        private void SetFill(float pressureFraction)
        {
            _fill.style.width = Length.Percent(pressureFraction.ToPercent());
            _percent.text = $"{Mathf.RoundToInt(pressureFraction.ToPercent())}%";
        }

        private void SetState(bool isOpen)
        {
            _gauge.EnableInClassList(ClosedClass, !isOpen);
            _state.text = isOpen ? "OPEN" : "CLOSED";
        }
    }
}
