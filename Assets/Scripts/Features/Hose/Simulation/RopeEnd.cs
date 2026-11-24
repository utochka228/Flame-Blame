namespace FlameBlame.Hose.Simulation
{
    public enum RopeEndKind
    {
        Start = 0,
        Tail = 1
    }

    public readonly struct RopeEnd
    {
        public int Rope { get; }
        public RopeEndKind Kind { get; }

        public RopeEnd(int rope, RopeEndKind kind)
        {
            Rope = rope;
            Kind = kind;
        }
    }
}
