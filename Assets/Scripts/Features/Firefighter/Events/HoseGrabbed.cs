using UnityEngine;

namespace FlameBlame.Firefighter.Events
{
    public readonly struct HoseGrabbed
    {
        public Vector3 Position { get; }

        public HoseGrabbed(Vector3 position)
        {
            Position = position;
        }
    }
}
