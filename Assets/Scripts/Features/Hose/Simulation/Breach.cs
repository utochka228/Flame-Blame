namespace FlameBlame.Hose.Simulation
{
    public readonly struct Breach
    {
        public int FirstIndex { get; }
        public int LastIndex { get; }
        public float Loss { get; }
        public bool IsTaped { get; }

        public int Index => (FirstIndex + LastIndex) / 2;

        public Breach(int firstIndex, int lastIndex, float loss, bool isTaped)
        {
            FirstIndex = firstIndex;
            LastIndex = lastIndex;
            Loss = loss;
            IsTaped = isTaped;
        }
    }
}
