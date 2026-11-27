using System;

namespace FlameBlame.Audio.Simulation
{
    public static class VolumeCurve
    {
        public const float SilentDecibels = -80f;

        public static float ToDecibels(float sliderFraction)
        {
            var slider = Math.Clamp(sliderFraction, 0f, 1f);
            return Math.Max(SilentDecibels, 20f * (float)Math.Log10(slider * slider));
        }
    }
}
