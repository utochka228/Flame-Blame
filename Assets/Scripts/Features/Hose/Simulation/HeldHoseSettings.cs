namespace FlameBlame.Hose.Simulation
{
    public class HeldHoseSettings
    {
        public float UpstreamRigidityFraction { get; set; } = 0.5f;
        public float FreeRigidityFraction { get; set; } = 0.05f;
        public float UpstreamDampingMultiplier { get; set; } = 2.5f;
        public float FreeDampingMultiplier { get; set; } = 0.7f;
    }
}
