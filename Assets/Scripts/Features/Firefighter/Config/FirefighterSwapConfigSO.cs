using FlameBlame.Core.Config;
using UnityEngine;

namespace FlameBlame.Firefighter.Config
{
    [CreateAssetMenu(fileName = "FirefighterSwapConfig", menuName = "FlameBlame/Configs/Firefighter Swap")]
    public class FirefighterSwapConfigSO : ConfigSO
    {
        [field: Header("Forced swap")]
        [field: SerializeField] public SwapInterval Interval { get; private set; } = SwapInterval.Off;
        [field: SerializeField] [field: Range(1f, 10f)] public float WarningSeconds { get; private set; } = 4f;

        [field: Header("Visor")]
        [field: SerializeField] [field: Range(0.05f, 1f)] public float VisorDropSeconds { get; private set; } = 0.25f;
        [field: SerializeField] [field: Range(0.05f, 1f)] public float VisorLiftSeconds { get; private set; } = 0.35f;

        [field: Header("Warning ticks")]
        [field: SerializeField] [field: Range(0.1f, 2f)] public float SlowestTickSeconds { get; private set; } = 0.8f;
        [field: SerializeField] [field: Range(0.05f, 1f)] public float FastestTickSeconds { get; private set; } = 0.12f;

        public float IntervalSeconds => (int)Interval;
    }
}
