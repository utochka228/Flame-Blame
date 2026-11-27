namespace FlameBlame.Level.Events
{
    public readonly struct LevelCompleted
    {
        public float Seconds { get; }
        public float HoseMetres { get; }
        public int Swaps { get; }

        public LevelCompleted(float seconds, float hoseMetres, int swaps)
        {
            Seconds = seconds;
            HoseMetres = hoseMetres;
            Swaps = swaps;
        }
    }
}
