using System.Runtime.CompilerServices;
using UnityEngine;

namespace FlameBlame.Core.Extensions
{
    public static class Vector3Extensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsFinite(this Vector3 vector) => IsFinite(vector.x) && IsFinite(vector.y) && IsFinite(vector.z);

        public static float OffsetFromRay(this Vector3 point, Vector3 origin, Vector3 direction, float reach)
        {
            var toPoint = point - origin;
            var along = Vector3.Dot(toPoint, direction);
            if (along < 0f || along > reach) return float.MaxValue;

            return (toPoint - direction * along).magnitude;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool IsFinite(float value) => !float.IsNaN(value) && !float.IsInfinity(value);
    }
}
