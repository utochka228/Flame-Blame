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
        [field: SerializeField] public LayerMask GroundLayers { get; private set; } = ~0;
    }
}
