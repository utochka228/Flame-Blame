using FlameBlame.Core.Config;
using FlameBlame.Fire.Simulation;
using UnityEngine;

namespace FlameBlame.Fire.Config
{
    [CreateAssetMenu(fileName = "FireConfig", menuName = "FlameBlame/Configs/Fire")]
    public class FireConfigSO : ConfigSO
    {
        [field: Header("Simulation")]
        [field: SerializeField] [field: Range(0.1f, 10f)] public float DousePerVolume { get; private set; } = 1.5f;
        [field: SerializeField] [field: Range(0.5f, 8f)] public float HeatRadius { get; private set; } = 2.5f;

        [field: Header("Visuals")]
        [field: SerializeField] [field: Range(0f, 1f)] public float MinFlameScaleFraction { get; private set; } = 0.35f;
        [field: SerializeField] [field: Range(1f, 30f)] public float FlameFollowSharpness { get; private set; } = 6f;
        [field: SerializeField] [field: Range(0.05f, 2f)] public float ExtinguishSeconds { get; private set; } = 0.6f;

        public FireSettings CreateSettings()
        {
            var settings = new FireSettings();
            ApplyTo(settings);
            return settings;
        }

        public void ApplyTo(FireSettings settings)
        {
            settings.DousePerVolume = DousePerVolume;
            settings.HeatRadius = HeatRadius;
        }
    }
}
