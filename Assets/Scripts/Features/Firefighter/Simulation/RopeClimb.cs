using System.Collections.Generic;
using UnityEngine;

namespace FlameBlame.Firefighter.Simulation
{
    public class RopeClimb
    {
        private const float TopTolerance = 1e-3f;

        private readonly float _metersPerSecond;
        private float _approachSign = 1f;

        public float Index { get; private set; }
        public bool IsAtTop { get; private set; }

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

        private int CrestIndex(IReadOnlyList<Vector3> positions) =>
            RopeCrest.Find(positions, Mathf.Clamp(Mathf.RoundToInt(Index), 0, positions.Count - 1));

        private int LowerIndex(IReadOnlyList<Vector3> positions) => Mathf.Clamp(Mathf.FloorToInt(Index), 0, positions.Count - 2);
    }
}
