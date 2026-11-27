using System;
using System.Globalization;
using DG.Tweening;
using FlameBlame.Level.Events;
using FlameBlame.UI.Kit;
using UnityEngine;
using UnityEngine.UIElements;

namespace FlameBlame.UI.Hud
{
    public class LevelCompleteScreen : IDisposable
    {
        private const string HiddenClass = "end-screen--hidden";
        private const float RowStaggerSeconds = 0.12f;

        private static readonly Vector2 RowEnterOffset = new Vector2(-60f, 0f);

        private readonly VisualElement _screen;
        private readonly VisualElement _card;
        private readonly VisualElement[] _rows;
        private readonly VisualElement _hint;
        private readonly Label _key;
        private readonly Label _time;
        private readonly Label _hose;
        private readonly Label _swaps;
        private Sequence _show;
        private Tween _keyPulse;

        public LevelCompleteScreen(VisualElement root)
        {
            _screen = root.Q("endScreen");
            _card = root.Q("endCard");
            _rows = new[] { root.Q("endTimeRow"), root.Q("endHoseRow"), root.Q("endSwapsRow") };
            _hint = root.Q("endHint");
            _key = root.Q<Label>("endKey");
            _time = root.Q<Label>("endTimeValue");
            _hose = root.Q<Label>("endHoseValue");
            _swaps = root.Q<Label>("endSwapsValue");
        }

        public void Show(LevelCompleted result)
        {
            _time.text = FormatTime(result.Seconds);
            _hose.text = result.HoseMetres.ToString("0.0", CultureInfo.InvariantCulture) + " m";
            _swaps.text = result.Swaps.ToString(CultureInfo.InvariantCulture);
            _screen.RemoveFromClassList(HiddenClass);

            _show?.Kill();
            _show = DOTween.Sequence()
                .Insert(0f, UiTween.Fade(_screen, 0f, 1f, 0.25f, Ease.OutQuad))
                .Insert(0f, UiTween.Pop(_card, 0f, 0.8f, Ease.OutElastic));
            for (var index = 0; index < _rows.Length; index++)
            {
                var at = 0.35f + index * RowStaggerSeconds;
                _rows[index].style.opacity = 0f;
                _show.Insert(at, UiTween.SlideIn(_rows[index], RowEnterOffset, 0.3f))
                    .Insert(at, UiTween.Fade(_rows[index], 0f, 1f, 0.2f, Ease.OutQuad));
            }

            _hint.style.opacity = 0f;
            _show.Append(UiTween.Fade(_hint, 0f, 1f, 0.3f, Ease.OutQuad))
                .AppendCallback(() => UiTween.Restart(ref _keyPulse, UiTween.Pulse(_key, 1.2f, 0.45f)))
                .SetUpdate(true);
        }

        public void Dispose()
        {
            _show?.Kill();
            _keyPulse?.Kill();
        }

        private static string FormatTime(float seconds)
        {
            var whole = Mathf.FloorToInt(seconds);
            return $"{whole / 60}:{whole % 60:00}";
        }
    }
}
