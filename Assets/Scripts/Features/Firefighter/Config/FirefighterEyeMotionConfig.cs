using System;
using FlameBlame.Core.Extensions;
using UnityEngine;

namespace FlameBlame.Firefighter.Config
{
    [Serializable]
    public class FirefighterEyeMotionConfig
    {
        [field: Header("Landing dip")]
        [field: SerializeField] [field: Range(0f, 10f)] public float LandingMinFallSpeed { get; private set; } = 3f;
        [field: SerializeField] [field: Range(0f, 1f)] public float LandingKickPerFallSpeed { get; private set; } = 0.35f;
        [field: SerializeField] [field: Range(0f, 10f)] public float LandingMaxKick { get; private set; } = 5f;
        [field: SerializeField] [field: Range(5f, 40f)] public float SpringAngularFrequency { get; private set; } = 18f;
        [field: SerializeField] [field: Range(0.1f, 1f)] public float SpringDampingRatio { get; private set; } = 0.6f;

        [field: Header("Head bob")]
        [field: SerializeField] [field: Range(0.3f, 2f)] public float BobStepLength { get; private set; } = 1.3f;
        [field: SerializeField] [field: Range(0f, 0.1f)] public float BobVerticalAmplitude { get; private set; } = 0.03f;
        [field: SerializeField] [field: Range(0f, 0.1f)] public float BobLateralAmplitude { get; private set; } = 0.015f;
        [field: SerializeField] [field: Range(1f, 20f)] public float BobBlendSpeed { get; private set; } = 8f;

        [field: Header("Strafe tilt")]
        [field: SerializeField] [field: Range(0f, 5f)] public float StrafeTiltDegrees { get; private set; } = 1.5f;
        [field: SerializeField] [field: Range(1f, 20f)] public float TiltBlendSpeed { get; private set; } = 8f;

        [field: Header("Speed FOV")]
        [field: SerializeField] [field: Range(0f, 20f)] public float FovExtraDegrees { get; private set; } = 8f;
        [field: SerializeField] [field: Range(1f, 30f)] public float FovFullSpeed { get; private set; } = 12f;
        [field: SerializeField] [field: Range(1f, 20f)] public float FovBlendSpeed { get; private set; } = 5f;

        [field: Header("Shake")]
        [field: SerializeField] [field: Range(0f, 20f)] public float LandingShakeMinFallSpeed { get; private set; } = 10f;
        [field: SerializeField] [field: Range(0f, 0.2f)] public float LandingTraumaPerFallSpeed { get; private set; } = 0.05f;
        [field: SerializeField] [field: Range(0.5f, 5f)] public float TraumaDecay { get; private set; } = 1.5f;
        [field: SerializeField] [field: Range(5f, 60f)] public float ShakeFrequency { get; private set; } = 25f;
        [field: SerializeField] [field: Range(0f, 0.2f)] public float ShakeMaxOffset { get; private set; } = 0.05f;
        [field: SerializeField] [field: Range(0f, 10f)] public float ShakeMaxRollDegrees { get; private set; } = 2f;

        [field: Header("Comfort")]
        [field: SerializeField] [field: Range(0f, 100f)] public float ShakeStrengthPercent { get; private set; } = 100f;
        [field: SerializeField] public bool IsCameraMotionReduced { get; private set; }

        public float ShakeStrengthFraction => ShakeStrengthPercent.ToFraction();
    }
}
