using System.Runtime.CompilerServices;

namespace FlameBlame.Core.Extensions
{
    public static class PercentExtensions
    {
        private const float Scale = 100f;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float ToFraction(this float percent) => percent / Scale;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float ToFraction(this int percent) => percent / Scale;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float ToPercent(this float fraction) => fraction * Scale;
    }
}
