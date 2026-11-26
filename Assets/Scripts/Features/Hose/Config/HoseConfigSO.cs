using FlameBlame.Core.Config;
using FlameBlame.Hose.Simulation;
using UnityEngine;

namespace FlameBlame.Hose.Config
{
    [CreateAssetMenu(fileName = "HoseConfig", menuName = "FlameBlame/Configs/Hose")]
    public class HoseConfigSO : ConfigSO
    {
        [field: SerializeField] public HoseRopeConfig Rope { get; private set; } = new();
        [field: SerializeField] public HosePressureConfig Pressure { get; private set; } = new();
        [field: SerializeField] public HosePinchConfig Pinch { get; private set; } = new();
        [field: SerializeField] public HoseBurstConfig Burst { get; private set; } = new();
        [field: SerializeField] public HoseHeatConfig Heat { get; private set; } = new();
        [field: SerializeField] public HoseHeldConfig Held { get; private set; } = new();
        [field: SerializeField] public HoseBridgeConfig Bridge { get; private set; } = new();
        [field: SerializeField] public HoseTubeConfig Tube { get; private set; } = new();
        [field: SerializeField] public HoseColorStyle Colors { get; private set; } = new();

        public int ParticleCount => Rope.ParticleCount;
        public float Length => Rope.Length;

        public RopeSettings CreateRopeSettings()
        {
            var settings = new RopeSettings { ParticleCount = Rope.ParticleCount };
            ApplyTo(settings);
            return settings;
        }

        public void ApplyTo(RopeSettings settings)
        {
            Rope.ApplyTo(settings);
            Pinch.ApplyTo(settings);
        }

        public HeldHoseSettings CreateHeldSettings()
        {
            var settings = new HeldHoseSettings();
            Held.ApplyTo(settings);
            return settings;
        }

        public BreachSettings CreateBreachSettings()
        {
            var settings = new BreachSettings { ParticleCount = ParticleCount };
            ApplyTo(settings);
            return settings;
        }

        public void ApplyTo(BreachSettings settings)
        {
            settings.WeakPointIndex = Mathf.RoundToInt(Burst.WeakPointFraction * (ParticleCount - 1));
            settings.OverloadSeconds = Burst.OverloadSeconds;
            settings.RecoverSeconds = Burst.RecoverSeconds;
            settings.HeatThreshold = Heat.BurnThresholdFraction;
            settings.BurnSeconds = Heat.BurnSeconds;
            settings.BurstLeakLoss = Burst.LeakLoss;
            settings.TapeLeakLoss = Burst.TapeLeakLoss;
        }
    }
}
