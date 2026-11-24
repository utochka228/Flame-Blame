namespace FlameBlame.Firefighter.Simulation
{
    public static class UseResolver
    {
        public static UseAction Resolve(UseContext context)
        {
            if (context.CanConnectHeldEnd) return UseAction.Connect;
            if (context.BurstInReach) return UseAction.Tape;
            if (context.JointInReach) return UseAction.Disconnect;
            if (context.ValveInReach) return UseAction.ToggleValve;
            return UseAction.None;
        }
    }
}
