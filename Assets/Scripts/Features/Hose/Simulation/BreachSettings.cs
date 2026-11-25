namespace FlameBlame.Hose.Simulation
{
    public class BreachSettings
    {
        public int ParticleCount { get; set; }
        public int WeakPointIndex { get; set; }
        public float OverloadSeconds { get; set; }
        public float RecoverSeconds { get; set; }
        public float HeatThreshold { get; set; }
        public float BurnSeconds { get; set; }
        public float BurstLeakLoss { get; set; }
        public float TapeLeakLoss { get; set; }
    }
}
