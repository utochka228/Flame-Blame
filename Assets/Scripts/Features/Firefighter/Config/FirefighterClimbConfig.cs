using System;
using FlameBlame.Core.Extensions;
using UnityEngine;

namespace FlameBlame.Firefighter.Config
{
    [Serializable]
    public class FirefighterClimbConfig
    {
        [field: SerializeField] [field: Range(0.5f, 5f)] public float Speed { get; private set; } = 2f;
        [field: SerializeField] [field: Range(0f, 1f)] public float StandOffMeters { get; private set; } = 0.25f;
        [field: SerializeField] [field: Range(0f, 100f)] public float MaxPressurePercent { get; private set; } = 20f;

        public float MaxPressureFraction => MaxPressurePercent.ToFraction();
    }
}
