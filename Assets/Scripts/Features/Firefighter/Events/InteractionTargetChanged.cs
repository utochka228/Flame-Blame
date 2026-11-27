using FlameBlame.Firefighter.Simulation;

namespace FlameBlame.Firefighter.Events
{
    public readonly struct InteractionTargetChanged
    {
        public UseAction Action { get; }

        public InteractionTargetChanged(UseAction action)
        {
            Action = action;
        }
    }
}
