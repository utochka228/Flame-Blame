namespace FlameBlame.Firefighter.Simulation
{
    public static class UseResolver
    {
        public static UseAction Resolve(UseContext context)
        {
            if (context.IsClimbing) return UseAction.StopClimb;
            if (context.CanConnectHeldEnd) return UseAction.Connect;
            if (context.BurstInReach) return UseAction.Tape;
            if (context.CanTieHeld) return UseAction.Tie;
            if (context.CanLatchHeld) return UseAction.Latch;
            if (context.TiedInReach) return UseAction.Untie;
            if (context.LatchedInReach) return UseAction.Unlatch;
            if (context.JointInReach) return UseAction.Disconnect;
            if (context.ValveInReach) return UseAction.ToggleValve;
            if (context.DoorInReach) return UseAction.ToggleDoor;
            if (context.RopeInReach) return UseAction.Climb;
            return UseAction.None;
        }
    }
}
