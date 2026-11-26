using System;
using System.Collections.Generic;
using DG.Tweening;
using FlameBlame.UI.Kit;
using UnityEngine;
using UnityEngine.UIElements;

namespace FlameBlame.UI.Hud
{
    public class EventFeedView : IDisposable
    {
        private const string ToastClass = "fb-toast";
        private const string GoodClass = "fb-toast--good";
        private const string DangerClass = "fb-toast--danger";
        private const int MaxToasts = 4;
        private const float EnterSeconds = 0.3f;
        private const float HoldSeconds = 3f;
        private const float ExitSeconds = 0.25f;

        private static readonly Vector2 EnterOffset = new Vector2(80f, 0f);
        private static readonly Vector2 ExitOffset = new Vector2(40f, 0f);

        private readonly VisualElement _feed;
        private readonly List<Sequence> _toasts = new List<Sequence>();

        public EventFeedView(VisualElement root)
        {
            _feed = root.Q("feed");
        }

        public void Push(string text, FeedTone tone)
        {
            if (_toasts.Count >= MaxToasts)
                _toasts[0].Kill();

            var toast = new Label(text) { pickingMode = PickingMode.Ignore };
            toast.style.opacity = 0f;
            toast.AddToClassList(ToastClass);
            toast.EnableInClassList(GoodClass, tone == FeedTone.Good);
            toast.EnableInClassList(DangerClass, tone == FeedTone.Danger);
            _feed.Add(toast);

            var sequence = DOTween.Sequence()
                .Insert(0f, UiTween.Slide(toast, EnterOffset, Vector2.zero, EnterSeconds, Ease.OutBack))
                .Insert(0f, UiTween.Fade(toast, 0f, 1f, EnterSeconds * 0.6f, Ease.OutQuad));
            if (tone == FeedTone.Danger)
                sequence.Insert(EnterSeconds, UiTween.Shake(toast, 10f, 0.4f));
            sequence.AppendInterval(HoldSeconds)
                .Append(UiTween.Fade(toast, 1f, 0f, ExitSeconds, Ease.InQuad))
                .Join(UiTween.Slide(toast, Vector2.zero, ExitOffset, ExitSeconds, Ease.InQuad))
                .SetUpdate(true);
            sequence.OnKill(() =>
            {
                _toasts.Remove(sequence);
                toast.RemoveFromHierarchy();
            });
            _toasts.Add(sequence);
        }

        public void Dispose()
        {
            for (var index = _toasts.Count - 1; index >= 0; index--)
                _toasts[index].Kill();
        }
    }
}
