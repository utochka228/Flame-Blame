using FlameBlame.Core.Config;
using UnityEngine;

namespace FlameBlame.Firefighter.Config
{
    [CreateAssetMenu(fileName = "FirefighterConfig", menuName = "FlameBlame/Configs/Firefighter")]
    public class FirefighterConfigSO : ConfigSO
    {
        [field: SerializeField] public FirefighterMovementConfig Movement { get; private set; } = new();
        [field: SerializeField] public FirefighterLookConfig Look { get; private set; } = new();
        [field: SerializeField] public FirefighterInteractionConfig Interaction { get; private set; } = new();
        [field: SerializeField] public FirefighterGripConfig Grip { get; private set; } = new();
        [field: SerializeField] public FirefighterThrowConfig Throw { get; private set; } = new();
        [field: SerializeField] public FirefighterClimbConfig Climb { get; private set; } = new();
        [field: SerializeField] public FirefighterTieConfig Tie { get; private set; } = new();
    }
}
