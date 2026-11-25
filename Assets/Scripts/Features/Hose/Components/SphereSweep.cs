using UnityEngine;

namespace FlameBlame.Hose.Components
{
    public static class SphereSweep
    {
        public static Vector3 Slide(Vector3 from, Vector3 to, float probeRadius, LayerMask layers)
        {
            var delta = to - from;
            var distance = delta.magnitude;
            if (distance < 1e-6f) return to;

            var direction = delta / distance;
            if (!Physics.SphereCast(from, probeRadius, direction, out var hit, distance, layers, QueryTriggerInteraction.Ignore))
                return to;

            var stop = from + direction * hit.distance;
            var remaining = to - stop;
            remaining -= hit.normal * Mathf.Min(0f, Vector3.Dot(remaining, hit.normal));
            return stop + remaining;
        }
    }
}
