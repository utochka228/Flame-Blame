using UnityEngine;

namespace FlameBlame.Hose.Simulation
{
    public class HeldHoseProfile
    {
        private readonly HeldHoseSettings _settings;

        public HeldHoseProfile(HeldHoseSettings settings)
        {
            _settings = settings;
        }

        public static void Clear(float[] rigidityCaps, float[] dampingMultipliers)
        {
            for (var index = 0; index < rigidityCaps.Length; index++)
            {
                rigidityCaps[index] = 1f;
                dampingMultipliers[index] = 1f;
            }
        }

        public void Apply(int heldIndex, float[] rigidityCaps, float[] dampingMultipliers)
        {
            var lastIndex = rigidityCaps.Length - 1;
            var freeFraction = (float)(lastIndex - heldIndex) / lastIndex;
            var tipRigidity = Mathf.Lerp(_settings.UpstreamRigidityFraction, _settings.FreeRigidityFraction, freeFraction);
            var tipDamping = Mathf.Lerp(_settings.UpstreamDampingMultiplier, _settings.FreeDampingMultiplier, freeFraction);

            for (var index = 0; index <= lastIndex; index++)
            {
                var distanceFraction = index <= heldIndex ? 0f : (float)(index - heldIndex) / (lastIndex - heldIndex);
                rigidityCaps[index] = Mathf.Lerp(_settings.UpstreamRigidityFraction, tipRigidity, distanceFraction);
                dampingMultipliers[index] = Mathf.Lerp(_settings.UpstreamDampingMultiplier, tipDamping, distanceFraction);
            }
        }
    }
}
