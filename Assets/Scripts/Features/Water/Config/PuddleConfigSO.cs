using FlameBlame.Core.Extensions;
using FlameBlame.Water.Simulation;
using UnityEngine;

namespace FlameBlame.Water.Config
{
    [CreateAssetMenu(fileName = "PuddleConfig", menuName = "FlameBlame/Configs/Puddle")]
    public class PuddleConfigSO : ScriptableObject
    {
        [field: SerializeField, Range(1, 64)] public int Capacity { get; private set; } = 24;
        [field: SerializeField, Range(0.05f, 1f)] public float RadiusPerSqrtVolume { get; private set; } = 0.35f;
        [field: SerializeField, Range(0f, 1f)] public float MergeRadius { get; private set; } = 0.35f;
        [field: SerializeField, Range(0f, 1f)] public float EvaporationPerSecond { get; private set; } = 0.05f;
        [field: SerializeField, Range(0f, 100f)] public float MinTractionPercent { get; private set; } = 25f;
        [field: SerializeField] public LayerMask GroundLayers { get; private set; } = ~0;
        [field: SerializeField, Range(0.5f, 10f)] public float MaxDropMeters { get; private set; } = 3f;

        public float MinTractionFraction => MinTractionPercent.ToFraction();

        public PuddleSettings CreateSettings() => new PuddleSettings
        {
            Capacity = Capacity,
            RadiusPerSqrtVolume = RadiusPerSqrtVolume,
            MergeRadius = MergeRadius,
            EvaporationPerSecond = EvaporationPerSecond,
            MinTraction = MinTractionFraction
        };
    }
}
