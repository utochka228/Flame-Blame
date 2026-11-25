using System;
using FlameBlame.Core.Extensions;
using FlameBlame.Hose.Simulation;
using UnityEngine;

namespace FlameBlame.Hose.Config
{
    [Serializable]
    public class HoseHeldConfig
    {
        [field: SerializeField] [field: Range(0f, 100f)] public float UpstreamRigidityPercent { get; private set; } = 50f;
        [field: SerializeField] [field: Range(0f, 100f)] public float FreeRigidityPercent { get; private set; } = 5f;
        [field: SerializeField] [field: Range(0.1f, 10f)] public float UpstreamDampingMultiplier { get; private set; } = 2.5f;
        [field: SerializeField] [field: Range(0.1f, 10f)] public float FreeDampingMultiplier { get; private set; } = 0.7f;
        [field: SerializeField] [field: Range(10f, 100f)] public float SoloCarrySpeedPercent { get; private set; } = 100f;

        public float SoloCarrySpeedFraction => SoloCarrySpeedPercent.ToFraction();

        public void ApplyTo(HeldHoseSettings settings)
        {
            settings.UpstreamRigidityFraction = UpstreamRigidityPercent.ToFraction();
            settings.FreeRigidityFraction = FreeRigidityPercent.ToFraction();
            settings.UpstreamDampingMultiplier = UpstreamDampingMultiplier;
            settings.FreeDampingMultiplier = FreeDampingMultiplier;
        }
    }
}
