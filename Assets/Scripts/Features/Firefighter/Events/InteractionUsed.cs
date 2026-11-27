using FlameBlame.Firefighter.Simulation;

namespace FlameBlame.Firefighter.Events
{
    public readonly struct InteractionUsed
    {
        public UseAction Action { get; }

        public InteractionUsed(UseAction action)
        {
            Action = action;
        }
    }
}
