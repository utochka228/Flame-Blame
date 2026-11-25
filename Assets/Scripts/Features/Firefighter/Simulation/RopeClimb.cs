using System.Collections.Generic;
using UnityEngine;

namespace FlameBlame.Firefighter.Simulation
{
    public class RopeClimb
    {
        private const float TopTolerance = 1e-3f;
        private const float LookaheadMeters = 0.5f;
        private const float MinRiseMeters = 0.15f;

        private readonly float _metersPerSecond;

        public float Index { get; private set; }
        public bool IsAtTop { get; private set; }

        private float _approachSign = 1f;

        public RopeClimb(float metersPerSecond)
        {
            _metersPerSecond = metersPerSecond;
        }

        public void Begin(int index)
        {
            Index = index;
            IsAtTop = false;
        }

        public Vector3 Step(IReadOnlyList<Vector3> positions, float input, float deltaTime)
        {
            var crest = CrestIndex(positions);
            var lower = LowerIndex(positions);
            var length = Mathf.Max((positions[lower + 1] - positions[lower]).magnitude, 1e-4f);
            var direction = ClimbDirection(positions, crest, input);
            var next = Index + direction * input * _metersPerSecond * deltaTime / length;
            var previous = Index;
            Index = Mathf.Clamp(Crosses(Index, next, crest) ? crest : next, 0f, positions.Count - 1);
            if (!Mathf.Approximately(Index, previous))
                _approachSign = Mathf.Sign(Index - previous);

            IsAtTop = Mathf.Abs(Index - crest) < TopTolerance;
            return Position(positions);
        }

        public Vector3 Position(IReadOnlyList<Vector3> positions)
        {
            var lower = LowerIndex(positions);
            return Vector3.Lerp(positions[lower], positions[lower + 1], Index - lower);
        }

        public Vector3 TopDirection(IReadOnlyList<Vector3> positions)
        {
            var crest = CrestIndex(positions);
            var far = crest + (int)_approachSign;
            if (far < 0 || far >= positions.Count)
                far = crest - (int)_approachSign;

            var across = positions[far] - positions[crest];
            across.y = 0f;
            return across.normalized;
        }

        private float ClimbDirection(IReadOnlyList<Vector3> positions, int crest, float input)
        {
            if (Index < crest - TopTolerance) return 1f;
            if (Index > crest + TopTolerance) return -1f;
            if (input >= 0f) return 0f;

            var hasBelow = crest > 0;
            var hasAbove = crest < positions.Count - 1;
            if (!hasBelow) return -1f;
            if (!hasAbove) return 1f;

            return positions[crest - 1].y < positions[crest + 1].y ? 1f : -1f;
        }

        private static bool Crosses(float from, float to, int crest) => (from - crest) * (to - crest) < 0f;

        private int CrestIndex(IReadOnlyList<Vector3> positions)
        {
            var crest = Mathf.Clamp(Mathf.RoundToInt(Index), 0, positions.Count - 1);
            var peak = PeakIndex(positions);
            if (peak == crest) return crest;

            var start = crest;
            var step = peak > crest ? 1 : -1;
            var climbed = Rises(positions, crest - step, step);
            while (crest != peak)
            {
                var rises = Rises(positions, crest, step);
                if (climbed && !rises) break;

                climbed |= rises;
                crest += step;
            }

            return climbed ? crest : start;
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

        private int LowerIndex(IReadOnlyList<Vector3> positions) => Mathf.Clamp(Mathf.FloorToInt(Index), 0, positions.Count - 2);
    }
}
