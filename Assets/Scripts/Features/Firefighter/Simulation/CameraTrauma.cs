using UnityEngine;

namespace FlameBlame.Firefighter.Simulation
{
    public class CameraTrauma
    {
        private float _phase;

        public float Trauma { get; private set; }
        public float Shake => Trauma * Trauma;

        public void Add(float amount)
        {
            Trauma = Mathf.Clamp01(Trauma + amount);
        }

        public void Step(float deltaTime, float decayPerSecond, float frequency)
        {
            Trauma = Mathf.Max(Trauma - decayPerSecond * deltaTime, 0f);
            _phase = Trauma > 0f ? _phase + frequency * deltaTime : 0f;
        }

        public float Wave(float rate) => Shake * Mathf.Sin(_phase * rate);
    }
}
