using System.Collections.Generic;

namespace FlameBlame.Pressure.Simulation
{
    public class PressureGraph
    {
        public const int SourceOutlet = 0;
        public const int HoseStart = 0;
        public const int HoseEnd = 1;

        internal const int PortsPerPart = 3;

        private readonly List<Part> _parts = new List<Part>();
        private readonly List<float> _thumbBoostFractions = new List<float>();
        private readonly List<PortLink> _links = new List<PortLink>();
        private readonly List<HoseMarker> _pinches = new List<HoseMarker>();
        private readonly List<HoseMarker> _leaks = new List<HoseMarker>();

        internal IReadOnlyList<Part> Parts => _parts;
        internal IReadOnlyList<PortLink> Links => _links;
        internal IReadOnlyList<HoseMarker> Pinches => _pinches;
        internal IReadOnlyList<HoseMarker> Leaks => _leaks;

        public void Clear()
        {
            _parts.Clear();
            _thumbBoostFractions.Clear();
            _links.Clear();
            _pinches.Clear();
            _leaks.Clear();
        }

        public int AddSource(float pressure) => AddPart(new Part(PartKind.Source, pressure, 0f, 0f));

        public int AddHose(float length, float lossPerMeter) => AddPart(new Part(PartKind.Hose, 0f, length, lossPerMeter));

        public int AddSplitter() => AddPart(new Part(PartKind.Splitter, 0f, 0f, 0f));

        public void Link(int partA, int portA, int partB, int portB)
        {
            _links.Add(new PortLink(new PortRef(partA, portA), new PortRef(partB, portB)));
        }

        public void Pinch(int hose, float position, float surgeFraction = 0f)
        {
            _pinches.Add(new HoseMarker(hose, position, surgeFraction));
        }

        public void Leak(int hose, float position, float loss)
        {
            _leaks.Add(new HoseMarker(hose, position, loss));
        }

        public void Thumb(int hose, float boostFraction)
        {
            _thumbBoostFractions[hose] = boostFraction;
        }

        internal float ThumbBoostFraction(int hose) => _thumbBoostFractions[hose];

        private int AddPart(Part part)
        {
            _parts.Add(part);
            _thumbBoostFractions.Add(0f);
            return _parts.Count - 1;
        }

        internal enum PartKind
        {
            Source = 0,
            Hose = 1,
            Splitter = 2
        }

        internal readonly struct Part
        {
            public PartKind Kind { get; }
            public float Pressure { get; }
            public float Length { get; }
            public float LossPerMeter { get; }

            public Part(PartKind kind, float pressure, float length, float lossPerMeter)
            {
                Kind = kind;
                Pressure = pressure;
                Length = length;
                LossPerMeter = lossPerMeter;
            }
        }

        internal readonly struct PortLink
        {
            public PortRef First { get; }
            public PortRef Second { get; }

            public PortLink(PortRef first, PortRef second)
            {
                First = first;
                Second = second;
            }
        }

        internal readonly struct HoseMarker
        {
            public int Hose { get; }
            public float Position { get; }
            public float Amount { get; }

            public HoseMarker(int hose, float position, float amount)
            {
                Hose = hose;
                Position = position;
                Amount = amount;
            }
        }
    }
}
