namespace FlameBlame.Pressure.Simulation
{
    public static class PressureSolver
    {
        public static void Solve(PressureGraph graph, PressureResult result)
        {
            result.Reset(graph);
            for (var index = 0; index < graph.Links.Count; index++)
                result.Connect(graph.Links[index].First, graph.Links[index].Second);

            for (var part = 0; part < graph.Parts.Count; part++)
            {
                if (graph.Parts[part].Kind != PressureGraph.PartKind.Source) continue;

                var neighbor = result.Neighbor(part, PressureGraph.SourceOutlet);
                if (!neighbor.IsNone)
                    Propagate(graph, result, neighbor, graph.Parts[part].Pressure);
            }
        }

        private static void Propagate(PressureGraph graph, PressureResult result, PortRef inlet, float pressure)
        {
            switch (graph.Parts[inlet.Part].Kind)
            {
                case PressureGraph.PartKind.Hose:
                    PropagateThroughHose(graph, result, inlet, pressure);
                    break;
                case PressureGraph.PartKind.Splitter:
                    PropagateThroughSplitter(graph, result, inlet, pressure);
                    break;
            }
        }

        private static void PropagateThroughHose(PressureGraph graph, PressureResult result, PortRef inlet, float pressure)
        {
            if (!result.TryVisit(inlet.Part, inlet.Port, pressure)) return;

            var outletEnd = inlet.Port == PressureGraph.HoseStart ? PressureGraph.HoseEnd : PressureGraph.HoseStart;
            var neighbor = result.Neighbor(inlet.Part, outletEnd);
            if (neighbor.IsNone) return;

            var outletPosition = outletEnd == PressureGraph.HoseEnd ? 1f : 0f;
            Propagate(graph, result, neighbor, result.Sample(inlet.Part, outletPosition));
        }

        private static void PropagateThroughSplitter(PressureGraph graph, PressureResult result, PortRef inlet, float pressure)
        {
            if (!result.TryVisit(inlet.Part, inlet.Port, pressure)) return;

            var outputCount = 0;
            for (var port = 0; port < PressureGraph.PortsPerPart; port++)
            {
                if (port != inlet.Port && !result.Neighbor(inlet.Part, port).IsNone)
                    outputCount++;
            }

            if (outputCount == 0) return;

            var share = pressure / outputCount;
            for (var port = 0; port < PressureGraph.PortsPerPart; port++)
            {
                var neighbor = result.Neighbor(inlet.Part, port);
                if (port != inlet.Port && !neighbor.IsNone)
                    Propagate(graph, result, neighbor, share);
            }
        }
    }
}
