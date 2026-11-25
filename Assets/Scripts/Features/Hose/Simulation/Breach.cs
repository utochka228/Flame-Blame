namespace FlameBlame.Hose.Simulation
{
    public readonly struct Breach
    {
        public int Index { get; }
        public float Loss { get; }
        public bool IsTaped { get; }

        public Breach(int index, float loss, bool isTaped)
        {
            Index = index;
            Loss = loss;
            IsTaped = isTaped;
        }
    }
}
