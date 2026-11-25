using UnityEngine;

namespace FlameBlame.Firefighter.Simulation
{
    public static class ThrowSpread
    {
        public static float Weight(int offset, int spreadParticles)
        {
            return Mathf.Max(0f, 1f - Mathf.Abs(offset) / (float)(spreadParticles + 1));
        }
    }
}
