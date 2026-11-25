using System;
using FlameBlame.Core.Extensions;
using UnityEngine;

namespace FlameBlame.Hose.Config
{
    [Serializable]
    public class HoseBridgeConfig
    {
        [field: SerializeField] [field: Range(0f, 100f)] public float PressurePercent { get; private set; } = 60f;
        [field: SerializeField] [field: Range(0.05f, 0.4f)] public float RadiusMeters { get; private set; } = 0.18f;
        [field: SerializeField] [field: Range(0.5f, 10f)] public float MaxSpanSpeed { get; private set; } = 2.5f;

        public float PressureFraction => PressurePercent.ToFraction();
    }
}
