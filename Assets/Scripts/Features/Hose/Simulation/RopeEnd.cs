using System;

namespace FlameBlame.Hose.Simulation
{
    public enum RopeEndKind
    {
        Start = 0,
        Tail = 1
    }

    public readonly struct RopeEnd : IEquatable<RopeEnd>
    {
        public int Rope { get; }
        public RopeEndKind Kind { get; }

        public RopeEnd(int rope, RopeEndKind kind)
        {
            Rope = rope;
            Kind = kind;
        }

        public bool Equals(RopeEnd other) => Rope == other.Rope && Kind == other.Kind;

        public override bool Equals(object obj) => obj is RopeEnd other && Equals(other);

        public override int GetHashCode() => Rope * 2 + (int)Kind;
    }
}
