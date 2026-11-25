using UnityEngine;

namespace FlameBlame.Hose.Components
{
    public static class SurfaceSideResolver
    {
        private const float FreeSideTolerance = 1e-3f;

        public static bool ShouldExitOppositeSide(Vector3 position, Vector3 lastFreePosition, Vector3 direction, float distance, float radius)
        {
            return distance > radius && Vector3.Dot(direction, lastFreePosition - position) < -FreeSideTolerance;
        }

        public static Vector3 ExitOppositeSide(Collider candidate, Vector3 position, Vector3 direction, float radius)
        {
            var reach = candidate.bounds.size.magnitude + radius;
            var ray = new Ray(position - direction * reach, direction);
            return candidate.Raycast(ray, out var hit, reach * 2f) ? hit.point - direction * radius : position;
        }
    }
}
