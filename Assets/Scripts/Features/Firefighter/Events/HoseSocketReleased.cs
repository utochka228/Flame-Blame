using UnityEngine;

namespace FlameBlame.Firefighter.Events
{
    public readonly struct HoseSocketReleased
    {
        public Vector3 Position { get; }

        public HoseSocketReleased(Vector3 position)
        {
            Position = position;
        }
    }
}
