using System.Collections.Generic;
using UnityEngine;

namespace FlameBlame.Hose.Simulation
{
    public class BreachPaint
    {
        private const float HalfParticle = 0.5f;

        private readonly Color _burnedColor;
        private readonly Color _tapedColor;
        private readonly float _featherParticles;

        public BreachPaint(Color burnedColor, Color tapedColor, float featherParticles)
        {
            _burnedColor = burnedColor;
            _tapedColor = tapedColor;
            _featherParticles = featherParticles;
        }

        public void Fill(Color[] texels, IReadOnlyList<Breach> breaches, int particleCount, Color baseColor)
        {
            var lastTexel = texels.Length - 1;
            var lastParticle = particleCount - 1;
            for (var texel = 0; texel < texels.Length; texel++)
            {
                var position = (float)texel / lastTexel * lastParticle;
                var color = baseColor;
                for (var index = 0; index < breaches.Count; index++)
                {
                    var weight = Coverage(breaches[index], position);
                    if (weight > 0f)
                        color = Color.Lerp(color, breaches[index].IsTaped ? _tapedColor : _burnedColor, weight);
                }

                texels[texel] = color;
            }
        }

        private float Coverage(Breach breach, float position)
        {
            var distance = Mathf.Max(breach.FirstIndex - HalfParticle - position, position - (breach.LastIndex + HalfParticle), 0f);
            if (_featherParticles <= 0f)
                return distance <= 0f ? 1f : 0f;

            return Mathf.Clamp01(1f - distance / _featherParticles);
        }
    }
}
