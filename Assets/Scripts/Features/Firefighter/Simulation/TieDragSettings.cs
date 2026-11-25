namespace FlameBlame.Firefighter.Simulation
{
    public class TieDragSettings
    {
        public float MinTension { get; set; } = 500f;
        public float SpeedMultiplier { get; set; } = 0.0005f;
        public float MaxSpeed { get; set; } = 3f;
        public float Acceleration { get; set; } = 60f;
    }
}
