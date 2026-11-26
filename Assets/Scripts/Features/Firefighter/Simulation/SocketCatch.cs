using System.Collections.Generic;
using UnityEngine;

namespace FlameBlame.Firefighter.Simulation
{
    public static class SocketCatch
    {
        public const int NoParticle = -1;

        public static CatchParts PartOf(int index, int count)
        {
            if (index == 0) return CatchParts.Start;
            return index == count - 1 ? CatchParts.Tail : CatchParts.Middle;
        }

        public static int Closest(IReadOnlyList<Vector3> positions, CatchParts allowed, Vector3 socket, float radius)
        {
            var best = NoParticle;
            var bestSqrDistance = radius * radius;
            var stride = (allowed & CatchParts.Middle) != 0 ? 1 : Mathf.Max(1, positions.Count - 1);
            for (var index = 0; index < positions.Count; index += stride)
            {
                if ((allowed & PartOf(index, positions.Count)) == 0) continue;

                var sqrDistance = (positions[index] - socket).sqrMagnitude;
                if (sqrDistance > bestSqrDistance) continue;

                best = index;
                bestSqrDistance = sqrDistance;
            }

            return best;
        }
    }
}
