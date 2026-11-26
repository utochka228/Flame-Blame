using System;
using DG.Tweening;
using FlameBlame.UI.Kit;
using UnityEngine.UIElements;

namespace FlameBlame.UI.Hud
{
    public class ReticleView : IDisposable
    {
        private const string ActiveClass = "hud-reticle--active";

        private readonly VisualElement _reticle;
        private bool _isActive;
        private Tween _pop;

        public ReticleView(VisualElement root)
        {
            _reticle = root.Q("reticle");
        }

        public void SetActive(bool isActive)
        {
            if (isActive == _isActive) return;

            _isActive = isActive;
            _reticle.EnableInClassList(ActiveClass, isActive);
            Pop(isActive ? 1.4f : 0.8f);
        }

        public void Press()
        {
            Pop(0.7f);
        }

        public void Dispose()
        {
            _pop?.Kill();
        }

        private void Pop(float fromScale)
        {
            UiTween.Restart(ref _pop, UiTween.Pop(_reticle, fromScale, 0.25f));
        }
    }
}
