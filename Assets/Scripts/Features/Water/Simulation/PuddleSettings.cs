namespace FlameBlame.Water.Simulation
{
    public class PuddleSettings
    {
        public int Capacity { get; set; }
        public float RadiusPerSqrtVolume { get; set; }
        public float MergeRadius { get; set; }
        public float EvaporationPerSecond { get; set; }
        public float MinTraction { get; set; }
    }
}
