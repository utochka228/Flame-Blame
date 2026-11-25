using System;
using FlameBlame.Core.Extensions;
using UnityEngine;

namespace FlameBlame.Hose.Config
{
    [Serializable]
    public class HoseHeatConfig
    {
        [field: SerializeField] [field: Range(1, 30)] public int ChecksPerSecond { get; private set; } = 10;
        [field: SerializeField] [field: Range(0f, 100f)] public float BurnThresholdPercent { get; private set; } = 50f;
        [field: SerializeField] [field: Range(0.2f, 20f)] public float BurnSeconds { get; private set; } = 3f;
        [field: SerializeField] [field: Range(0.2f, 20f)] public float CouplingBurnSeconds { get; private set; } = 2f;

        public float BurnThresholdFraction => BurnThresholdPercent.ToFraction();
    }
}
