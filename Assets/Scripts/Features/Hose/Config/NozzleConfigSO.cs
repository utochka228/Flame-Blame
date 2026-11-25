using FlameBlame.Core.Extensions;
using FlameBlame.Hose.Simulation;
using UnityEngine;

namespace FlameBlame.Hose.Config
{
    [CreateAssetMenu(fileName = "NozzleConfig", menuName = "FlameBlame/Configs/Nozzle")]
    public class NozzleConfigSO : ScriptableObject
    {
        [field: SerializeField, Range(1f, 15f)] public float MaxJetLength { get; private set; } = 6f;
        [field: SerializeField, Range(0f, 30f)] public float BaseSpreadDegrees { get; private set; } = 6f;
        [field: SerializeField, Range(0f, 90f)] public float ThumbSpreadDegrees { get; private set; } = 40f;
        [field: SerializeField, Range(0f, 60f)] public float RecoilAcceleration { get; private set; } = 25f;
        [field: SerializeField, Range(0f, 100f)] public float ThumbBoostPercent { get; private set; } = 50f;
        [field: SerializeField, Range(0f, 200f)] public float PushForce { get; private set; } = 40f;
        [field: SerializeField, Range(0f, 2f)] public float JetPuddleVolumePerSecond { get; private set; } = 0.3f;
        [field: SerializeField, Range(0.02f, 0.5f)] public float PushRadius { get; private set; } = 0.15f;
        [field: SerializeField] public LayerMask PushLayers { get; private set; } = ~0;
        [field: SerializeField, Range(0.1f, 2f)] public float ParticleLifetime { get; private set; } = 0.6f;
        [field: SerializeField, Range(0f, 1000f)] public float ParticlesPerSecondAtFullFlow { get; private set; } = 300f;

        public float ThumbBoostFraction => ThumbBoostPercent.ToFraction();

        public NozzleModel CreateModel() => new NozzleModel(MaxJetLength, BaseSpreadDegrees, ThumbSpreadDegrees, RecoilAcceleration);
    }
}
