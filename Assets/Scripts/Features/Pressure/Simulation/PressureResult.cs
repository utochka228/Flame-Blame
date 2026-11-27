using System.Collections.Generic;
using UnityEngine;

namespace FlameBlame.Pressure.Simulation
{
    public class PressureResult
    {
        private const int NoInlet = -1;

        private readonly List<float> _inletPressures = new List<float>();
        private readonly List<int> _inletEnds = new List<int>();
        private readonly List<PortRef> _neighbors = new List<PortRef>();

        private PressureGraph _graph;

        public float Sample(int hose, float position)
        {
            var inletEnd = _inletEnds[hose];
            if (inletEnd == NoInlet) return 0f;

            var part = _graph.Parts[hose];
            var distanceFraction = DistanceFromInlet(inletEnd, position);
            var pressure = _inletPressures[hose] - part.LossPerMeter * part.Length * distanceFraction;

            var surgeFraction = 0f;
            for (var index = 0; index < _graph.Pinches.Count; index++)
            {
                var pinch = _graph.Pinches[index];
                if (pinch.Hose != hose) continue;
                if (DistanceFromInlet(inletEnd, pinch.Position) <= distanceFraction)
                    return 0f;

                surgeFraction = Mathf.Max(surgeFraction, pinch.Amount);
            }

            for (var index = 0; index < _graph.Leaks.Count; index++)
            {
                var leak = _graph.Leaks[index];
                if (leak.Hose == hose && DistanceFromInlet(inletEnd, leak.Position) <= distanceFraction)
                    pressure -= leak.Amount;
            }

            return Mathf.Max(0f, pressure) * (1f + surgeFraction);
        }

        public bool IsFed(int hose) => _inletEnds[hose] != NoInlet;

        public float TipPressure(int hose)
        {
            if (_inletEnds[hose] == NoInlet) return 0f;

            var tipPosition = _inletEnds[hose] == PressureGraph.HoseStart ? 1f : 0f;
            return Sample(hose, tipPosition) * (1f + _graph.ThumbBoostFraction(hose));
        }

        internal void Reset(PressureGraph graph)
        {
            _graph = graph;
            _inletPressures.Clear();
            _inletEnds.Clear();
            _neighbors.Clear();
            for (var index = 0; index < graph.Parts.Count; index++)
            {
                _inletPressures.Add(0f);
                _inletEnds.Add(NoInlet);
            }

            for (var index = 0; index < graph.Parts.Count * PressureGraph.PortsPerPart; index++)
                _neighbors.Add(PortRef.None);
        }

        internal void Connect(PortRef first, PortRef second)
        {
            _neighbors[SlotOf(first)] = second;
            _neighbors[SlotOf(second)] = first;
        }

        internal PortRef Neighbor(int part, int port) => _neighbors[SlotOf(new PortRef(part, port))];

        internal bool TryVisit(int part, int inletEnd, float inletPressure)
        {
            if (_inletEnds[part] != NoInlet) return false;

            _inletEnds[part] = inletEnd;
            _inletPressures[part] = inletPressure;
            return true;
        }

        private static int SlotOf(PortRef port) => port.Part * PressureGraph.PortsPerPart + port.Port;

        private static float DistanceFromInlet(int inletEnd, float position) =>
            inletEnd == PressureGraph.HoseStart ? position : 1f - position;
    }
}
