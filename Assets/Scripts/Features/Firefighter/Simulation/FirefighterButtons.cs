using System;

namespace FlameBlame.Firefighter.Simulation
{
    [Flags]
    public enum FirefighterButtons
    {
        None = 0,
        Jump = 1 << 0,
        Grab = 1 << 1,
        Throw = 1 << 2,
        Thumb = 1 << 3,
        Use = 1 << 4
    }
}
