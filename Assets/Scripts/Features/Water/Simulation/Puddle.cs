using UnityEngine;

namespace FlameBlame.Water.Simulation
{
    public readonly struct Puddle
    {
        public Vector3 Position { get; }
        public float Volume { get; }
        public float Radius { get; }
        public float Opacity { get; }

        public Puddle(Vector3 position, float volume, float radius, float opacity)
        {
            Position = position;
            Volume = volume;
            Radius = radius;
            Opacity = opacity;
        }

        public bool IsFading => Opacity < 1f;
    }
}
