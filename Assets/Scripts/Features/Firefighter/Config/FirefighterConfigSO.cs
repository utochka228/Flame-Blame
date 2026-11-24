using UnityEngine;

namespace FlameBlame.Firefighter.Config
{
    [CreateAssetMenu(fileName = "FirefighterConfig", menuName = "FlameBlame/Configs/Firefighter")]
    public class FirefighterConfigSO : ScriptableObject
    {
        [field: SerializeField, Range(1f, 10f)] public float MoveSpeed { get; private set; } = 5f;
        [field: SerializeField, Range(1f, 100f)] public float Acceleration { get; private set; } = 40f;
        [field: SerializeField, Range(1f, 15f)] public float JumpSpeed { get; private set; } = 6f;
        [field: SerializeField, Range(0.01f, 0.5f)] public float LookDegreesPerPixel { get; private set; } = 0.1f;
        [field: SerializeField, Range(30f, 89f)] public float PitchLimitDegrees { get; private set; } = 85f;
        [field: SerializeField, Range(0.01f, 0.5f)] public float GroundCheckDistance { get; private set; } = 0.1f;
        [field: SerializeField, Range(1f, 6f)] public float ReachMeters { get; private set; } = 3f;
        [field: SerializeField, Range(0.1f, 1f)] public float AimRadiusMeters { get; private set; } = 0.4f;
        [field: SerializeField, Range(0.5f, 3f)] public float HoldDistanceMeters { get; private set; } = 1.5f;
        [field: SerializeField, Min(1e-9f)] public float GripCompliance { get; private set; } = 1e-7f;
        [field: SerializeField, Range(0f, 0.5f)] public float PullAccelerationMultiplier { get; private set; } = 0.05f;
        [field: SerializeField, Range(1f, 30f)] public float MaxPullAcceleration { get; private set; } = 6f;
        [field: SerializeField, Range(1f, 30f)] public float ThrowSpeed { get; private set; } = 12f;
        [field: SerializeField] public LayerMask GroundLayers { get; private set; } = ~0;
    }
}
