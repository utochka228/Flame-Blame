using FlameBlame.Core.Extensions;
using UnityEngine.UIElements;

namespace FlameBlame.UI.Hud
{
    public class SwapOverlayView
    {
        private readonly VisualElement _fog;
        private readonly VisualElement _visor;

        public SwapOverlayView(VisualElement root)
        {
            _fog = root.Q("swapFog");
            _visor = root.Q("swapVisor");
        }

        public void Show(float warningFraction, float coverFraction)
        {
            _fog.style.opacity = warningFraction;
            _visor.style.translate = new Translate(0f, Length.Percent((coverFraction - 1f).ToPercent()));
        }
    }
}
