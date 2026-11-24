using System.Collections.Generic;
using UnityEngine;

namespace FlameBlame.Hose.Presentation
{
    public class HoseRadiusProfile
    {
        private readonly float _baseRadius;
        private readonly float _swellFraction;
        private readonly float _pinchSquashFraction;

        public HoseRadiusProfile(float baseRadius, float swellFraction, float pinchSquashFraction)
        {
            _baseRadius = baseRadius;
            _swellFraction = swellFraction;
            _pinchSquashFraction = pinchSquashFraction;
        }

        public void Fill(IReadOnlyList<float> pressureFractions, IReadOnlyList<bool> pinched, float[] radii)
        {
            for (var index = 0; index < radii.Length; index++)
            {
                var radius = _baseRadius * (1f + _swellFraction * pressureFractions[index]);
                radii[index] = pinched[index] ? radius * (1f - _pinchSquashFraction) : radius;
            }
        }

        public static float FalloffWeight(int index, int centerIndex, float widthParticles) =>
            Mathf.Max(0f, 1f - Mathf.Abs(index - centerIndex) / widthParticles);

        public void AddBulge(float[] radii, int centerIndex, float widthParticles, float extraFraction)
        {
            for (var index = 0; index < radii.Length; index++)
                radii[index] *= 1f + extraFraction * FalloffWeight(index, centerIndex, widthParticles);
        }
    }
}
