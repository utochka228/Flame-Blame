namespace FlameBlame.Firefighter.Simulation
{
    public readonly struct UseContext
    {
        public bool CanConnectHeldEnd { get; }
        public bool BurstInReach { get; }
        public bool JointInReach { get; }
        public bool ValveInReach { get; }

        public UseContext(bool canConnectHeldEnd, bool burstInReach, bool jointInReach, bool valveInReach)
        {
            CanConnectHeldEnd = canConnectHeldEnd;
            BurstInReach = burstInReach;
            JointInReach = jointInReach;
            ValveInReach = valveInReach;
        }
    }
}
