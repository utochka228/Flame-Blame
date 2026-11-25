using System;
using FlameBlame.Core.Extensions;
using UnityEngine;

namespace FlameBlame.Hose.Config
{
    [Serializable]
    public class HosePressureConfig
    {
        [field: Header("Flow")]
        [field: SerializeField] [field: Min(1f)] public float MaxPressure { get; private set; } = 100f;
        [field: SerializeField] [field: Min(0f)] public float LossPerMeter { get; private set; } = 2f;
        [field: SerializeField] [field: Range(0f, 200f)] public float SmoothingPercentPerSecond { get; private set; } = 100f;

        [field: Header("Open end")]
        [field: SerializeField] [field: Min(0f)] public float OpenEndLeakLoss { get; private set; } = 40f;
        [field: SerializeField] [field: Min(0f)] public float OpenEndRecoilAcceleration { get; private set; } = 6f;
        [field: SerializeField] [field: Min(0f)] public float LeakPuddleVolumePerSecond { get; private set; } = 0.4f;

        public float SmoothingFractionPerSecond => SmoothingPercentPerSecond.ToFraction();
    }
}
