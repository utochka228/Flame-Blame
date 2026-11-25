using FlameBlame.Fire.Simulation;
using UnityEngine;

namespace FlameBlame.Fire.Config
{
    [CreateAssetMenu(fileName = "FireConfig", menuName = "FlameBlame/Configs/Fire")]
    public class FireConfigSO : ScriptableObject
    {
        [field: SerializeField, Range(0.1f, 10f)] public float DousePerVolume { get; private set; } = 1.5f;
        [field: SerializeField, Range(0.5f, 8f)] public float HeatRadius { get; private set; } = 2.5f;
        [field: SerializeField, Range(0f, 500f)] public float FlameParticlesPerSecond { get; private set; } = 120f;

        public FireSettings CreateSettings() => new FireSettings
        {
            DousePerVolume = DousePerVolume,
            HeatRadius = HeatRadius
        };
    }
}
