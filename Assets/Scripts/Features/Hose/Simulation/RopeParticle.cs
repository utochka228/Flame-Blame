namespace FlameBlame.Hose.Simulation
{
    public readonly struct RopeParticle
    {
        public static readonly RopeParticle None = new RopeParticle(-1, -1);

        public int Rope { get; }
        public int Index { get; }
        public bool IsNone => Rope < 0;

        public RopeParticle(int rope, int index)
        {
            Rope = rope;
            Index = index;
        }
    }
}
