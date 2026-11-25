namespace FlameBlame.Firefighter.Simulation
{
    public static class CarryLoad
    {
        private const int NozzleGripCount = 2;
        private const int SingleGripCount = 1;

        public static float SpeedScale(int activeGripsOnHose, bool holdsNozzle, float soloSpeedFraction)
        {
            var ownGrips = holdsNozzle ? NozzleGripCount : SingleGripCount;
            return activeGripsOnHose > ownGrips ? 1f : soloSpeedFraction;
        }
    }
}
