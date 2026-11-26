using System;

namespace FlameBlame.Firefighter.Simulation
{
    [Flags]
    public enum CatchParts
    {
        None = 0,
        Start = 1 << 0,
        Tail = 1 << 1,
        Middle = 1 << 2,
        Ends = Start | Tail,
        All = Start | Tail | Middle
    }
}
