using FlameBlame.Core.Config;
using FlameBlame.Interactables.Simulation;
using UnityEngine;

namespace FlameBlame.Interactables.Config
{
    [CreateAssetMenu(fileName = "DoorConfig", menuName = "FlameBlame/Configs/Door")]
    public class DoorConfigSO : ConfigSO
    {
        [field: Header("Motion")]
        [field: SerializeField] [field: Range(10f, 170f)] public float OpenAngleDegrees { get; private set; } = 100f;
        [field: SerializeField] [field: Range(10f, 360f)] public float OpenDegreesPerSecond { get; private set; } = 120f;
        [field: SerializeField] [field: Range(10f, 1000f)] public float CloseDegreesPerSecond { get; private set; } = 500f;

        [field: Header("Slam")]
        [field: SerializeField] [field: Range(0f, 1f)] public float SlamPaddingMeters { get; private set; } = 0.3f;

        public DoorModel CreateModel() => new DoorModel(OpenAngleDegrees, OpenDegreesPerSecond, CloseDegreesPerSecond);

        public void ApplyTo(DoorModel model) => model.Configure(OpenAngleDegrees, OpenDegreesPerSecond, CloseDegreesPerSecond);
    }
}
