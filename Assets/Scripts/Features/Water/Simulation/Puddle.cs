using UnityEngine;

namespace FlameBlame.Water.Simulation
{
    public readonly struct Puddle
    {
        public Vector3 Position { get; }
        public float Volume { get; }
        public float Radius { get; }

        public Puddle(Vector3 position, float volume, float radius)
        {
            Position = position;
            Volume = volume;
            Radius = radius;
        }
    }
}
