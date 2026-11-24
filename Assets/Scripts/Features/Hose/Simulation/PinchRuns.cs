using System.Collections.Generic;

namespace FlameBlame.Hose.Simulation
{
    public static class PinchRuns
    {
        public static void CollectPositions(IReadOnlyList<bool> pinched, List<float> positions)
        {
            positions.Clear();
            var lastIndex = pinched.Count - 1;
            for (var index = 0; index < pinched.Count; index++)
            {
                var startsRun = pinched[index] && (index == 0 || !pinched[index - 1]);
                if (startsRun)
                    positions.Add((float)index / lastIndex);
            }
        }
    }
}
