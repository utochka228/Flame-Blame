using System;
using FlameBlame.Core.Extensions;
using UnityEngine;

namespace FlameBlame.Hose.Config
{
    [Serializable]
    public class HoseBurstConfig
    {
        [field: Header("Sequence")]
        [field: SerializeField] [field: Range(0.2f, 10f)] public float OverloadSeconds { get; private set; } = 1.5f;
        [field: SerializeField] [field: Range(0.2f, 10f)] public float RecoverSeconds { get; private set; } = 1f;
        [field: SerializeField] [field: Range(0f, 100f)] public float WeakPointPercent { get; private set; } = 50f;

        [field: Header("Leak")]
        [field: SerializeField] [field: Min(0f)] public float LeakLoss { get; private set; } = 40f;
        [field: SerializeField] [field: Min(0f)] public float TapeLeakLoss { get; private set; } = 8f;

        [field: Header("Warning visuals")]
        [field: SerializeField] [field: Range(0f, 300f)] public float BalloonPercent { get; private set; } = 120f;
        [field: SerializeField] [field: Range(1f, 20f)] public float BalloonWidthParticles { get; private set; } = 6f;
        [field: SerializeField] [field: Range(0f, 0.2f)] public float ShakeAmplitude { get; private set; } = 0.04f;
        [field: SerializeField] [field: Range(1f, 60f)] public float ShakeHertz { get; private set; } = 25f;

        public float WeakPointFraction => WeakPointPercent.ToFraction();
        public float BalloonFraction => BalloonPercent.ToFraction();
    }
}
