using System;
using DG.Tweening;
using FlameBlame.UI.Kit;
using UnityEngine;
using UnityEngine.UIElements;

namespace FlameBlame.UI.Hud
{
    public class ObjectiveView : IDisposable
    {
        private const string DoneClass = "hud-objective--done";
        private const string DoneText = "All fires out!";

        private static readonly Color FlashColor = new Color(0.38f, 0.88f, 0.42f, 0.9f);

        private readonly VisualElement _chip;
        private readonly Label _text;
        private readonly Label _count;
        private int _shownFiresLeft = -1;
        private bool _isDone;
        private Tween _countPop;
        private Tween _chipFlash;
        private Tween _chipPop;

        public ObjectiveView(VisualElement root)
        {
            _chip = root.Q("objective");
            _text = root.Q<Label>("objectiveText");
            _count = root.Q<Label>("objectiveCount");
        }

        public void Show(int firesLeft, bool isComplete)
        {
            if (isComplete)
            {
                ShowDone();
                return;
            }

            _count.text = $"{firesLeft} left";
            if (_shownFiresLeft >= 0 && firesLeft < _shownFiresLeft)
            {
                UiTween.Restart(ref _countPop, UiTween.Pop(_count, 1.6f, 0.35f));
                UiTween.Restart(ref _chipFlash, UiTween.Flash(_chip, FlashColor, 0.5f));
            }

            _shownFiresLeft = firesLeft;
        }

        public void Dispose()
        {
            _countPop?.Kill();
            _chipFlash?.Kill();
            _chipPop?.Kill();
        }

        private void ShowDone()
        {
            if (_isDone) return;

            _isDone = true;
            _text.text = DoneText;
            _chip.AddToClassList(DoneClass);
            UiTween.Restart(ref _chipPop, UiTween.Squash(_chip, new Vector2(1.3f, 0.7f), 0.6f, Ease.OutElastic));
        }
    }
}
