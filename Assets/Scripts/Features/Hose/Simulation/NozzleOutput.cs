namespace FlameBlame.Hose.Simulation
{
    public readonly struct NozzleOutput
    {
        public float FlowFraction { get; }
        public float JetLength { get; }
        public float SpreadDegrees { get; }
        public float RecoilAcceleration { get; }

        public NozzleOutput(float flowFraction, float jetLength, float spreadDegrees, float recoilAcceleration)
        {
            FlowFraction = flowFraction;
            JetLength = jetLength;
            SpreadDegrees = spreadDegrees;
            RecoilAcceleration = recoilAcceleration;
        }
    }
}
