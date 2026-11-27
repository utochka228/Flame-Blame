using FlameBlame.Core.Config;
using FlameBlame.Core.Extensions;
using FlameBlame.Water.Simulation;
using UnityEngine;

namespace FlameBlame.Water.Config
{
    [CreateAssetMenu(fileName = "PuddleConfig", menuName = "FlameBlame/Configs/Puddle")]
    public class PuddleConfigSO : ConfigSO
    {
        [field: Header("Shape")]
        [field: SerializeField] [field: Range(1, 64)] public int Capacity { get; private set; } = 24;
        [field: SerializeField] [field: Range(0.05f, 1f)] public float RadiusPerSqrtVolume { get; private set; } = 0.35f;
        [field: SerializeField] [field: Range(0f, 1f)] public float MergeRadius { get; private set; } = 0.35f;

        [field: Header("Behavior")]
        [field: SerializeField] [field: Range(0f, 1f)] public float EvaporationPerSecond { get; private set; } = 0.05f;
        [field: SerializeField] [field: Range(0f, 100f)] public float MinTractionPercent { get; private set; } = 25f;

        [field: Header("Placement")]
        [field: SerializeField] public LayerMask GroundLayers { get; private set; } = ~0;
        [field: SerializeField] [field: Range(0.5f, 10f)] public float MaxDropMeters { get; private set; } = 3f;
        [field: SerializeField] [field: Range(0.05f, 2f)] public float AirborneFadeSeconds { get; private set; } = 0.3f;

        public float MinTractionFraction => MinTractionPercent.ToFraction();

        public PuddleSettings CreateSettings()
        {
            var settings = new PuddleSettings();
            ApplyTo(settings);
            return settings;
        }

        public void ApplyTo(PuddleSettings settings)
        {
            settings.Capacity = Capacity;
            settings.RadiusPerSqrtVolume = RadiusPerSqrtVolume;
            settings.MergeRadius = MergeRadius;
            settings.EvaporationPerSecond = EvaporationPerSecond;
            settings.MinTraction = MinTractionFraction;
        }
    }
}
