using System;
using FlameBlame.Core.Extensions;
using FlameBlame.Hose.Simulation;
using UnityEngine;

namespace FlameBlame.Hose.Config
{
    [Serializable]
    public class HosePinchConfig
    {
        [field: SerializeField] public LayerMask Layers { get; private set; }
        [field: SerializeField] public LayerMask FootLayers { get; private set; }
        [field: SerializeField] [field: Range(0.05f, 1f)] public float FootHeightMeters { get; private set; } = 0.35f;
        [field: SerializeField] [field: Range(1, 30)] public int ChecksPerSecond { get; private set; } = 10;
        [field: SerializeField] [field: Range(0f, 100f)] public float SquashPercent { get; private set; } = 60f;
        [field: SerializeField] [field: Range(0f, 100f)] public float SurgePercent { get; private set; } = 40f;
        [field: SerializeField] [field: Range(0f, 3f)] public float SlipSeconds { get; private set; } = 0.5f;

        public float SquashFraction => SquashPercent.ToFraction();
        public float SurgeFraction => SurgePercent.ToFraction();

        public void ApplyTo(RopeSettings settings)
        {
            settings.SlipSeconds = SlipSeconds;
        }
    }
}
