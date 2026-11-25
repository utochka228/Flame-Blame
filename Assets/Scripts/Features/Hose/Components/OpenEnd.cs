using FlameBlame.Hose.Simulation;

namespace FlameBlame.Hose.Components
{
    public class OpenEnd
    {
        public RopeEndKind Kind { get; }
        public int ParticleIndex { get; }
        public int InnerIndex { get; }
        public bool IsOpen { get; set; }

        public OpenEnd(RopeEndKind kind, int particleIndex, int innerIndex)
        {
            Kind = kind;
            ParticleIndex = particleIndex;
            InnerIndex = innerIndex;
        }
    }
}

