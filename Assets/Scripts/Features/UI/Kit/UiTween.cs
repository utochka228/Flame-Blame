using DG.Tweening;
using UnityEngine;
using UnityEngine.UIElements;

namespace FlameBlame.UI.Kit
{
    // DOTween helpers for UI Toolkit. Every tween runs in real time and, except Fade, puts the
    // element back on its USS rest value when it ends or is killed.
    public static class UiTween
    {
        public static Tween Pop(VisualElement element, float fromScale, float duration, Ease ease = Ease.OutBack) =>
            Squash(element, new Vector2(fromScale, fromScale), duration, ease);

        public static Tween Squash(VisualElement element, Vector2 fromScale, float duration, Ease ease = Ease.OutBack)
        {
            var scale = fromScale;
            SetScale(element, scale);
            return DOTween.To(() => scale, value =>
                {
                    scale = value;
                    SetScale(element, value);
                }, Vector2.one, duration)
                .SetEase(ease)
                .SetUpdate(true)
                .OnKill(() => element.style.scale = StyleKeyword.Null);
        }

        public static Tween Pulse(VisualElement element, float toScale, float halfPeriod)
        {
            var scale = 1f;
            return DOTween.To(() => scale, value =>
                {
                    scale = value;
                    SetScale(element, new Vector2(value, value));
                }, toScale, halfPeriod)
                .SetEase(Ease.InOutSine)
                .SetLoops(-1, LoopType.Yoyo)
                .SetUpdate(true)
                .OnKill(() => element.style.scale = StyleKeyword.Null);
        }

        public static Tween Slide(VisualElement element, Vector2 fromPixels, Vector2 toPixels, float duration, Ease ease)
        {
            var offset = fromPixels;
            return DOTween.To(() => offset, value =>
                {
                    offset = value;
                    SetTranslate(element, value);
                }, toPixels, duration)
                .SetEase(ease)
                .OnStart(() => SetTranslate(element, fromPixels))
                .SetUpdate(true);
        }

        public static Tween SlideIn(VisualElement element, Vector2 fromPixels, float duration, Ease ease = Ease.OutBack) =>
            Slide(element, fromPixels, Vector2.zero, duration, ease)
                .OnKill(() => element.style.translate = StyleKeyword.Null);

        public static Tween Fade(VisualElement element, float from, float to, float duration, Ease ease)
        {
            var opacity = from;
            return DOTween.To(() => opacity, value =>
                {
                    opacity = value;
                    element.style.opacity = value;
                }, to, duration)
                .SetEase(ease)
                .OnStart(() => element.style.opacity = from)
                .SetUpdate(true);
        }

        public static Tween Shake(VisualElement element, float strengthPixels, float duration)
        {
            var offset = Vector3.zero;
            return DOTween.Shake(() => offset, value =>
                {
                    offset = value;
                    SetTranslate(element, value);
                }, duration, new Vector3(strengthPixels, strengthPixels * 0.3f, 0f), 18, 90f, true, ShakeRandomnessMode.Harmonic)
                .SetUpdate(true)
                .OnKill(() => element.style.translate = StyleKeyword.Null);
        }

        public static Tween Flash(VisualElement element, Color from, float duration)
        {
            var rest = element.resolvedStyle.backgroundColor;
            var color = from;
            element.style.backgroundColor = color;
            return DOTween.To(() => color, value =>
                {
                    color = value;
                    element.style.backgroundColor = value;
                }, rest, duration)
                .SetEase(Ease.OutQuad)
                .SetUpdate(true)
                .OnKill(() => element.style.backgroundColor = StyleKeyword.Null);
        }

        public static Tween Wobble(VisualElement element, float fromDegrees, float restDegrees, float duration)
        {
            var angle = fromDegrees;
            return DOTween.To(() => angle, value =>
                {
                    angle = value;
                    element.style.rotate = new Rotate(value);
                }, restDegrees, duration)
                .SetEase(Ease.OutElastic)
                .SetUpdate(true)
                .OnKill(() => element.style.rotate = StyleKeyword.Null);
        }

        public static void Restart(ref Tween tween, Tween next)
        {
            tween?.Kill();
            tween = next;
        }

        private static void SetScale(VisualElement element, Vector2 scale)
        {
            element.style.scale = new Scale(new Vector3(scale.x, scale.y, 1f));
        }

        private static void SetTranslate(VisualElement element, Vector2 offset)
        {
            element.style.translate = new Translate(offset.x, offset.y);
        }
    }
}
