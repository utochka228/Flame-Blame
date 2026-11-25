using System.Collections.Generic;
using UnityEngine;

namespace FlameBlame.Firefighter.Simulation
{
    public static class RopeCrest
    {
        private const float LookaheadMeters = 0.5f;
        private const float MinRiseMeters = 0.15f;

        public static int Find(IReadOnlyList<Vector3> positions, int origin)
        {
            var peak = PeakIndex(positions);
            if (peak == origin) return origin;

            var crest = origin;
            var step = peak > origin ? 1 : -1;
            var climbed = Rises(positions, crest - step, step);
            while (crest != peak)
            {
                var rises = Rises(positions, crest, step);
                if (climbed && !rises) break;

                climbed |= rises;
                crest += step;
            }

            return climbed ? crest : origin;
        }

        private static int PeakIndex(IReadOnlyList<Vector3> positions)
        {
            var peak = 0;
            for (var index = 1; index < positions.Count; index++)
            {
                if (positions[index].y > positions[peak].y)
                    peak = index;
            }

            return peak;
        }

        private static bool Rises(IReadOnlyList<Vector3> positions, int from, int step)
        {
            if (from < 0 || from >= positions.Count) return false;

            var best = positions[from].y;
            var distance = 0f;
            for (var index = from + step; index >= 0 && index < positions.Count && distance < LookaheadMeters; index += step)
            {
                distance += (positions[index] - positions[index - step]).magnitude;
                best = Mathf.Max(best, positions[index].y);
            }

            return best - positions[from].y >= MinRiseMeters;
        }
    }
}
