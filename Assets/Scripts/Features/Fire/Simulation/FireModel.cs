using System;
using UnityEngine;

namespace FlameBlame.Fire.Simulation
{
    public class FireModel
    {
        private readonly FireSettings _settings;

        public FireModel(FireSettings settings)
        {
            _settings = settings;
        }

        public float Intensity { get; private set; } = 1f;
        public bool IsOut { get; private set; }

        public event Action Extinguished;

        public void Douse(float volume)
        {
            if (IsOut || volume <= 0f) return;

            Intensity = Mathf.Max(0f, Intensity - volume * _settings.DousePerVolume);
            if (Intensity > 0f) return;

            IsOut = true;
            Extinguished?.Invoke();
        }

        public float HeatAt(float distance)
        {
            if (IsOut || distance >= _settings.HeatRadius) return 0f;

            return Intensity * (1f - distance / _settings.HeatRadius);
        }
    }
}
