using System;
using System.Collections.Generic;
using FlameBlame.Hose.Config;
using FlameBlame.Hose.Simulation;
using UnityEngine;

namespace FlameBlame.Hose.Presentation
{
    public class HoseTubePaint
    {
        private const int TexelsPerParticle = 8;
        private const float ChangeTolerance = 1f / 512f;

        private BreachPaint _paint;
        private readonly Color[] _texels;
        private readonly Color[] _uploaded;
        private readonly int _particleCount;

        public Texture2D Texture { get; }

        public HoseTubePaint(HoseConfigSO config)
        {
            _particleCount = config.ParticleCount;
            ApplyColors(config);
            _texels = new Color[_particleCount * TexelsPerParticle];
            _uploaded = new Color[_texels.Length];
            Texture = new Texture2D(_texels.Length, 1, TextureFormat.RGBA32, false)
            {
                name = "HoseTubePaint",
                wrapMode = TextureWrapMode.Clamp,
                filterMode = FilterMode.Bilinear
            };
        }

        public void ApplyColors(HoseConfigSO config)
        {
            _paint = new BreachPaint(config.Colors.BurnedColor, config.Colors.TapedColor, config.Colors.BreachFeatherParticles);
        }

        public void Update(IReadOnlyList<Breach> breaches, Color baseColor)
        {
            _paint.Fill(_texels, breaches, _particleCount, baseColor);
            if (!HasChanged()) return;

            Texture.SetPixels(_texels);
            Texture.Apply(false);
            Array.Copy(_texels, _uploaded, _texels.Length);
        }

        public void Destroy()
        {
            UnityEngine.Object.Destroy(Texture);
        }

        private bool HasChanged()
        {
            for (var index = 0; index < _texels.Length; index++)
            {
                var difference = _texels[index] - _uploaded[index];
                if (Mathf.Abs(difference.r) > ChangeTolerance || Mathf.Abs(difference.g) > ChangeTolerance || Mathf.Abs(difference.b) > ChangeTolerance)
                    return true;
            }

            return false;
        }
    }
}
