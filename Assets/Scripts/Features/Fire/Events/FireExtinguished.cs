using UnityEngine;

namespace FlameBlame.Fire.Events
{
    public readonly struct FireExtinguished
    {
        public Vector3 Position { get; }

        public FireExtinguished(Vector3 position)
        {
            Position = position;
        }
    }
}
