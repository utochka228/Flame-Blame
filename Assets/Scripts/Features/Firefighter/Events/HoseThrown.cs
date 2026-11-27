using UnityEngine;

namespace FlameBlame.Firefighter.Events
{
    public readonly struct HoseThrown
    {
        public Vector3 Position { get; }

        public HoseThrown(Vector3 position)
        {
            Position = position;
        }
    }
}
