using UnityEngine;

namespace FlameBlame.Interactables.Events
{
    public readonly struct DoorOpened
    {
        public Vector3 Position { get; }

        public DoorOpened(Vector3 position)
        {
            Position = position;
        }
    }
}
