using FlameBlame.Pressure.Components;

namespace FlameBlame.Firefighter.Events
{
    public readonly struct ValveAdjusted
    {
        public PressureSource Valve { get; }
        public bool IsAtLimit { get; }

        public ValveAdjusted(PressureSource valve, bool isAtLimit)
        {
            Valve = valve;
            IsAtLimit = isAtLimit;
        }
    }
}
