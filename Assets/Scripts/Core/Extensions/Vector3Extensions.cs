using System.Runtime.CompilerServices;
using UnityEngine;

namespace FlameBlame.Core.Extensions
{
    public static class Vector3Extensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsFinite(this Vector3 vector) => IsFinite(vector.x) && IsFinite(vector.y) && IsFinite(vector.z);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool IsFinite(float value) => !float.IsNaN(value) && !float.IsInfinity(value);
    }
}
