using System;
using DG.Tweening;
using FlameBlame.UI.Kit;
using UnityEngine.UIElements;

namespace FlameBlame.UI.Hud
{
    public class LevelCompleteBannerView : IDisposable
    {
        private const string HiddenClass = "hud-banner--hidden";
        private const float HoldSeconds = 2.5f;

        private readonly VisualElement _banner;
        private Sequence _show;

        public LevelCompleteBannerView(VisualElement root)
        {
            _banner = root.Q("banner");
        }

        public void Show()
        {
            _show?.Kill();
            _banner.RemoveFromClassList(HiddenClass);
            _show = DOTween.Sequence()
                .Insert(0f, UiTween.Pop(_banner, 0f, 0.8f, Ease.OutElastic))
                .Insert(0f, UiTween.Fade(_banner, 0f, 1f, 0.2f, Ease.OutQuad))
                .AppendInterval(HoldSeconds)
                .Append(UiTween.Fade(_banner, 1f, 0f, 0.4f, Ease.InQuad))
                .SetUpdate(true)
                .OnKill(() => _banner.AddToClassList(HiddenClass));
        }

        public void Dispose()
        {
            _show?.Kill();
        }
    }
}
