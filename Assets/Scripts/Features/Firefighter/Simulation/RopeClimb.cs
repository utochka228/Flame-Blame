using System.Collections.Generic;
using UnityEngine;

namespace FlameBlame.Firefighter.Simulation
{
    public class RopeClimb
    {
        private const float TopTolerance = 1e-3f;

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
            var peak = PeakIndex(positions);
            var lower = LowerIndex(positions);
            var length = Mathf.Max((positions[lower + 1] - positions[lower]).magnitude, 1e-4f);
            var direction = ClimbDirection(positions, peak, input);
            var next = Index + direction * input * _metersPerSecond * deltaTime / length;
            var previous = Index;
            Index = Mathf.Clamp(Crosses(Index, next, peak) ? peak : next, 0f, positions.Count - 1);
            if (!Mathf.Approximately(Index, previous))
                _approachSign = Mathf.Sign(Index - previous);

            IsAtTop = Mathf.Abs(Index - peak) < TopTolerance;
            return Position(positions);
        }

        public Vector3 Position(IReadOnlyList<Vector3> positions)
        {
            var lower = LowerIndex(positions);
            return Vector3.Lerp(positions[lower], positions[lower + 1], Index - lower);
        }

        public Vector3 TopDirection(IReadOnlyList<Vector3> positions)
        {
            var peak = PeakIndex(positions);
            var far = peak + (int)_approachSign;
            if (far < 0 || far >= positions.Count)
                far = peak - (int)_approachSign;

            var across = positions[far] - positions[peak];
            across.y = 0f;
            return across.normalized;
        }

        private float ClimbDirection(IReadOnlyList<Vector3> positions, int peak, float input)
        {
            if (Index < peak - TopTolerance) return 1f;
            if (Index > peak + TopTolerance) return -1f;
            if (input >= 0f) return 0f;

            var hasBelow = peak > 0;
            var hasAbove = peak < positions.Count - 1;
            if (!hasBelow) return -1f;
            if (!hasAbove) return 1f;

            return positions[peak - 1].y < positions[peak + 1].y ? 1f : -1f;
        }

        private static bool Crosses(float from, float to, int peak) => (from - peak) * (to - peak) < 0f;

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

        private int LowerIndex(IReadOnlyList<Vector3> positions) => Mathf.Clamp(Mathf.FloorToInt(Index), 0, positions.Count - 2);
    }
}
