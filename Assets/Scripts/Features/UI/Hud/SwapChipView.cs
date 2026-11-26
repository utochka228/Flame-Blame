using System;
using DG.Tweening;
using FlameBlame.UI.Kit;
using UnityEngine;
using UnityEngine.UIElements;

namespace FlameBlame.UI.Hud
{
    public class SwapChipView : IDisposable
    {
        private const string WarnClass = "fb-keycap--warn";

        private readonly VisualElement _chip;
        private readonly Label _name;
        private readonly Label _key;
        private Tween _chipPop;
        private Tween _keyPulse;

        public SwapChipView(VisualElement root)
        {
            _chip = root.Q("swap");
            _name = root.Q<Label>("swapName");
            _key = root.Q<Label>("swapKey");
        }

        public void ShowActive(bool isFirstActive, bool animate)
        {
            _name.text = isFirstActive ? "Firefighter 1" : "Firefighter 2";
            if (!animate) return;

            UiTween.Restart(ref _chipPop, UiTween.Squash(_chip, new Vector2(1.2f, 0.8f), 0.4f));
        }

        public void ShowWarning(float warningFraction)
        {
            var isWarning = warningFraction > 0f;
            if (isWarning == (_keyPulse != null && _keyPulse.IsActive())) return;

            _key.EnableInClassList(WarnClass, isWarning);
            if (isWarning)
                _keyPulse = UiTween.Pulse(_key, 1.25f, 0.22f);
            else
                _keyPulse?.Kill();
        }

        public void Dispose()
        {
            _chipPop?.Kill();
            _keyPulse?.Kill();
        }
    }
}
