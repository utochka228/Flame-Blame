using UnityEngine;

namespace FlameBlame.Firefighter.Events
{
    public readonly struct HoseSocketLatched
    {
        public Vector3 Position { get; }

        public HoseSocketLatched(Vector3 position)
        {
            Position = position;
        }
    }
}
