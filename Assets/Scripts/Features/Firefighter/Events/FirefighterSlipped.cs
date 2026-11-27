using UnityEngine;

namespace FlameBlame.Firefighter.Events
{
    public readonly struct FirefighterSlipped
    {
        public Vector3 Position { get; }

        public FirefighterSlipped(Vector3 position)
        {
            Position = position;
        }
    }
}
