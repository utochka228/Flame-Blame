using System;
using DG.Tweening;
using FlameBlame.UI.Kit;
using UnityEngine;
using UnityEngine.UIElements;

namespace FlameBlame.UI.Menu
{
    public class BriefingWindow : IDisposable
    {
        private const string HiddenClass = "menu--hidden";
        private const string DotClass = "briefing__dot";
        private const string CurrentDotClass = "briefing__dot--current";
        private const string NextText = "Next";
        private const string LastText = "LET'S GO!";

        private static readonly BriefingStep[] Steps =
        {
            new BriefingStep("Congratulations, you're a firefighter",
                "The building is on fire. That's bad. You have a hose. That's good. Put out every fire before anyone asks who started them."),
            new BriefingStep("Controls (allegedly)",
                "WASD waddle · Space hop · E grab the hose · LMB fling it · F do the obvious thing · hold RMB or V to thumb the nozzle."),
            new BriefingStep("Water is angry",
                "More pressure means a stiffer hose, a longer jet and more recoil. Thumb the nozzle for a wider, sillier spray. Too much pressure and you fly backwards. That's a feature."),
            new BriefingStep("Hoses are balloons",
                "Kinks, slammed doors and fire make hoses burst. Look at the hole and press F to tape it. Duct tape fixes everything. Mostly."),
            new BriefingStep("You are two people",
                "Tab swaps bodies. The one you leave stays exactly where it was, probably standing on your hose. The swap timer may also swap you without asking. Blame them."),
            new BriefingStep("Get scored, get blamed",
                "When the last fire dies you get your time, hose used and swaps. Clever short routes beat long sad walks. Esc pauses. R replays. Go!")
        };

        private readonly VisualElement _window;
        private readonly VisualElement _card;
        private readonly Label _counter;
        private readonly Label _title;
        private readonly Label _body;
        private readonly Button _back;
        private readonly Button _next;
        private readonly VisualElement[] _dots;
        private int _index;
        private Sequence _open;
        private Tween _close;
        private Tween _titleSquash;
        private Tween _bodySlide;
        private Tween _bodyFade;
        private Tween _dotPop;

        public bool IsShown { get; private set; }

        public event Action Finished;

        public BriefingWindow(VisualElement root)
        {
            _window = root.Q("briefing");
            _card = root.Q("briefingCard");
            _counter = root.Q<Label>("briefingCounter");
            _title = root.Q<Label>("briefingTitle");
            _body = root.Q<Label>("briefingBody");
            _back = root.Q<Button>("briefingBack");
            _next = root.Q<Button>("briefingNext");
            var skip = root.Q<Button>("briefingSkip");

            var dots = root.Q("briefingDots");
            _dots = new VisualElement[Steps.Length];
            for (var index = 0; index < Steps.Length; index++)
            {
                _dots[index] = new VisualElement { pickingMode = PickingMode.Ignore };
                _dots[index].AddToClassList(DotClass);
                dots.Add(_dots[index]);
            }

            foreach (var button in new[] { _back, _next, skip })
                button.focusable = false;
            _back.clicked += Back;
            _next.clicked += Next;
            skip.clicked += Close;
        }

        public void Show()
        {
            IsShown = true;
            _index = 0;
            ShowStep(0f);
            _close?.Kill();
            _window.RemoveFromClassList(HiddenClass);
            _open?.Kill();
            _open = DOTween.Sequence()
                .Insert(0f, UiTween.Fade(_window, 0f, 1f, 0.2f, Ease.OutQuad))
                .Insert(0f, UiTween.Pop(_card, 0.5f, 0.45f))
                .Insert(0f, UiTween.Wobble(_card, -8f, 0f, 0.7f))
                .SetUpdate(true);
        }

        public void Next()
        {
            if (_index == Steps.Length - 1)
            {
                Close();
                return;
            }

            _index++;
            ShowStep(1f);
        }

        public void Dispose()
        {
            _open?.Kill();
            _close?.Kill();
            _titleSquash?.Kill();
            _bodySlide?.Kill();
            _bodyFade?.Kill();
            _dotPop?.Kill();
        }

        private void Back()
        {
            if (_index == 0) return;

            _index--;
            ShowStep(-1f);
        }

        private void Close()
        {
            if (!IsShown) return;

            IsShown = false;
            _open?.Kill();
            _close = DOTween.Sequence()
                .Insert(0f, UiTween.Squash(_card, new Vector2(1.15f, 0.6f), 0.25f, Ease.InBack))
                .Insert(0f, UiTween.Fade(_window, 1f, 0f, 0.25f, Ease.InQuad))
                .OnComplete(() => _window.AddToClassList(HiddenClass))
                .SetUpdate(true);
            Finished?.Invoke();
        }

        private void ShowStep(float direction)
        {
            var step = Steps[_index];
            _counter.text = $"{_index + 1} / {Steps.Length}";
            _title.text = step.Title;
            _body.text = step.Body;
            _back.SetEnabled(_index > 0);
            _next.text = _index == Steps.Length - 1 ? LastText : NextText;
            for (var index = 0; index < _dots.Length; index++)
                _dots[index].EnableInClassList(CurrentDotClass, index == _index);

            if (Mathf.Approximately(direction, 0f)) return;

            UiTween.Restart(ref _titleSquash, UiTween.Squash(_title, new Vector2(1.15f, 0.85f), 0.4f));
            UiTween.Restart(ref _bodySlide, UiTween.SlideIn(_body, new Vector2(60f * direction, 0f), 0.3f, Ease.OutCubic));
            UiTween.Restart(ref _bodyFade, UiTween.Fade(_body, 0f, 1f, 0.25f, Ease.OutQuad));
            UiTween.Restart(ref _dotPop, UiTween.Pop(_dots[_index], 1.8f, 0.3f));
        }
    }
}
