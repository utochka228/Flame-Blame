using FlameBlame.Core.Extensions;
using FlameBlame.Firefighter.Simulation;
using UnityEngine;

namespace FlameBlame.Firefighter.Config
{
    [CreateAssetMenu(fileName = "FirefighterConfig", menuName = "FlameBlame/Configs/Firefighter")]
    public class FirefighterConfigSO : ScriptableObject
    {
        [field: SerializeField, Range(1f, 10f)] public float MoveSpeed { get; private set; } = 5f;
        [field: SerializeField, Range(1f, 100f)] public float Acceleration { get; private set; } = 40f;
        [field: SerializeField, Range(1f, 100f)] public float Deceleration { get; private set; } = 10f;
        [field: SerializeField, Range(1f, 15f)] public float JumpSpeed { get; private set; } = 6f;
        [field: SerializeField, Range(0f, 100f)] public float AirControlPercent { get; private set; } = 15f;
        [field: SerializeField, Range(0.01f, 0.5f)] public float LookDegreesPerPixel { get; private set; } = 0.1f;
        [field: SerializeField, Range(30f, 89f)] public float PitchLimitDegrees { get; private set; } = 85f;
        [field: SerializeField, Range(0.01f, 0.5f)] public float GroundCheckDistance { get; private set; } = 0.1f;
        [field: SerializeField, Range(1f, 6f)] public float ReachMeters { get; private set; } = 3f;
        [field: SerializeField, Range(0.1f, 1f)] public float AimRadiusMeters { get; private set; } = 0.4f;
        [field: SerializeField, Range(0.5f, 3f)] public float HoldDistanceMeters { get; private set; } = 1.5f;
        [field: SerializeField, Range(1f, 10f)] public float MaxGripDistanceMeters { get; private set; } = 3.5f;
        [field: SerializeField, Range(0.2f, 2f)] public float ConnectRadiusMeters { get; private set; } = 0.6f;
        [field: SerializeField] public LayerMask InteractLayers { get; private set; } = ~0;
        [field: SerializeField, Min(1e-9f)] public float GripCompliance { get; private set; } = 1e-7f;
        [field: SerializeField, Min(1e-9f)] public float AimGripCompliance { get; private set; } = 1e-5f;
        [field: SerializeField, Range(0.01f, 0.5f)] public float GripFollowSeconds { get; private set; } = 0.12f;
        [field: SerializeField, Range(0f, 0.5f)] public float PullAccelerationMultiplier { get; private set; } = 0.05f;
        [field: SerializeField, Range(1f, 30f)] public float MaxPullAcceleration { get; private set; } = 6f;
        [field: SerializeField, Range(0f, 1f)] public float RecoilTransferMultiplier { get; private set; } = 1f;
        [field: SerializeField, Range(5f, 40f)] public float MaxSpeed { get; private set; } = 20f;
        [field: SerializeField, Range(1f, 30f)] public float ThrowSpeed { get; private set; } = 12f;
        [field: SerializeField] public LayerMask GroundLayers { get; private set; } = ~0;
        [field: SerializeField, Range(0.5f, 5f)] public float ClimbSpeed { get; private set; } = 2f;
        [field: SerializeField, Range(0f, 1f)] public float ClimbStandOffMeters { get; private set; } = 0.25f;
        [field: SerializeField, Range(0f, 100f)] public float ClimbMaxPressurePercent { get; private set; } = 20f;
        [field: SerializeField, Range(0f, 0.01f)] public float TieSpeedMultiplier { get; private set; } = 0.0005f;
        [field: SerializeField, Range(0f, 5000f)] public float TieMinTension { get; private set; } = 500f;
        [field: SerializeField, Range(0.1f, 3f)] public float TieBlendSeconds { get; private set; } = 0.6f;
        [field: SerializeField, Range(1f, 100f)] public float TieAcceleration { get; private set; } = 60f;
        [field: SerializeField, Range(0.5f, 10f)] public float MaxTieSpeed { get; private set; } = 3f;

        public float AirControlFraction => AirControlPercent.ToFraction();
        public float ClimbMaxPressureFraction => ClimbMaxPressurePercent.ToFraction();

        public TieDragSettings CreateTieDragSettings() => new TieDragSettings
        {
            MinTension = TieMinTension,
            SpeedMultiplier = TieSpeedMultiplier,
            MaxSpeed = MaxTieSpeed,
            Acceleration = TieAcceleration
        };
    }
}
