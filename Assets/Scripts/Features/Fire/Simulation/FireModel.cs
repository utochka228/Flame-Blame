using System;
using UnityEngine;

namespace FlameBlame.Fire.Simulation
{
    public class FireModel
    {
        private readonly FireSettings _settings;
        private float _dryTime;
        private float _pendingWater;

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

            _dryTime = 0f;
            Intensity = Mathf.Max(0f, Intensity - volume * _settings.DousePerVolume);
            if (Intensity > 0f) return;

            IsOut = true;
            Extinguished?.Invoke();
        }

        public void AddWater(float volume)
        {
            _pendingWater += volume;
        }

        public void Tick(float deltaTime)
        {
            var water = _pendingWater;
            _pendingWater = 0f;
            if (water >= _settings.MinWaterPerSecond * deltaTime)
                Douse(water);

            if (IsOut) return;

            _dryTime += deltaTime;
            var regrowTime = Mathf.Min(deltaTime, _dryTime - _settings.RegrowDelaySeconds);
            if (regrowTime <= 0f) return;

            Intensity = Mathf.Min(1f, Intensity + _settings.RegrowPerSecond * regrowTime);
        }

        public float HeatAt(float distance)
        {
            if (IsOut || distance >= _settings.HeatRadius) return 0f;

            return Intensity * (1f - distance / _settings.HeatRadius);
        }
    }
}
