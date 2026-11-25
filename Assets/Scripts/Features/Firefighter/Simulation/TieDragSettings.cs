namespace FlameBlame.Firefighter.Simulation
{
    public class TieDragSettings
    {
        public float MinTension { get; set; } = 500f;
        public float ForceMultiplier { get; set; } = 0.04f;
        public float MaxForce { get; set; } = 300f;
        public float MaxSpeed { get; set; } = 2f;
        public float BrakeSeconds { get; set; } = 0.25f;
    }
}
