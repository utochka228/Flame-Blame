using FlameBlame.Core.Extensions;
using FlameBlame.Hose.Simulation;
using UnityEngine;

namespace FlameBlame.Hose.Config
{
    [CreateAssetMenu(fileName = "HoseConfig", menuName = "FlameBlame/Configs/Hose")]
    public class HoseConfigSO : ScriptableObject
    {
        private static readonly RopeSettings Defaults = new RopeSettings();

        [field: SerializeField, Range(8, 120)] public int ParticleCount { get; private set; } = Defaults.ParticleCount;
        [field: SerializeField, Range(0.1f, 0.4f)] public float Spacing { get; private set; } = Defaults.Spacing;
        [field: SerializeField, Range(0.02f, 0.15f)] public float ParticleRadius { get; private set; } = Defaults.ParticleRadius;
        [field: SerializeField, Range(2, 16)] public int Substeps { get; private set; } = Defaults.Substeps;
        [field: SerializeField] public float FloppyBendCompliance { get; private set; } = Defaults.FloppyBendCompliance;
        [field: SerializeField] public float RigidBendCompliance { get; private set; } = Defaults.RigidBendCompliance;
        [field: SerializeField, Range(0f, 5f)] public float Damping { get; private set; } = Defaults.Damping;
        [field: SerializeField, Range(0f, 1f)] public float GroundFriction { get; private set; } = Defaults.GroundFriction;
        [field: SerializeField, Range(5f, 40f)] public float MaxSpeed { get; private set; } = Defaults.MaxSpeed;
        [field: SerializeField, Range(0.02f, 0.5f)] public float MaxStretchFraction { get; private set; } = Defaults.MaxStretchFraction;
        [field: SerializeField, Range(0f, 200f)] public float PressureSmoothingPercentPerSecond { get; private set; } = 100f;
        [field: SerializeField, Range(0f, 100f)] public float SwellPercent { get; private set; } = 40f;
        [field: SerializeField, Range(1, 8)] public int TubeSamplesPerSegment { get; private set; } = 4;
        [field: SerializeField, Range(6, 24)] public int TubeRingSides { get; private set; } = 12;
        [field: SerializeField, Range(1f, 3f)] public float TubeMiterLimit { get; private set; } = 1.5f;
        [field: SerializeField] public LayerMask CollisionLayers { get; private set; } = ~0;
        [field: SerializeField] public LayerMask PinchLayers { get; private set; }
        [field: SerializeField, Range(1, 30)] public int PinchChecksPerSecond { get; private set; } = 10;
        [field: SerializeField, Range(0f, 100f)] public float PinchSquashPercent { get; private set; } = 60f;
        [field: SerializeField, Range(0.2f, 10f)] public float BurstOverloadSeconds { get; private set; } = 1.5f;
        [field: SerializeField, Range(0.2f, 10f)] public float BurstRecoverSeconds { get; private set; } = 1f;
        [field: SerializeField, Range(0f, 100f)] public float BurstWeakPointPercent { get; private set; } = 50f;
        [field: SerializeField, Range(0f, 300f)] public float BalloonPercent { get; private set; } = 120f;
        [field: SerializeField, Range(1f, 20f)] public float BalloonWidthParticles { get; private set; } = 6f;
        [field: SerializeField, Range(0f, 0.2f)] public float ShakeAmplitude { get; private set; } = 0.04f;
        [field: SerializeField, Range(1f, 60f)] public float ShakeHertz { get; private set; } = 25f;
        [field: SerializeField] public Color RelaxedColor { get; private set; } = new Color(0.9f, 0.2f, 0.15f);
        [field: SerializeField] public Color StrainedColor { get; private set; } = new Color(1f, 0.6f, 0.1f);

        public float PressureSmoothingFractionPerSecond => PressureSmoothingPercentPerSecond.ToFraction();
        public float SwellFraction => SwellPercent.ToFraction();
        public float PinchSquashFraction => PinchSquashPercent.ToFraction();
        public float BurstWeakPointFraction => BurstWeakPointPercent.ToFraction();
        public float BalloonFraction => BalloonPercent.ToFraction();

        public RopeSettings CreateRopeSettings() => new RopeSettings
        {
            ParticleCount = ParticleCount,
            Spacing = Spacing,
            ParticleRadius = ParticleRadius,
            Substeps = Substeps,
            FloppyBendCompliance = FloppyBendCompliance,
            RigidBendCompliance = RigidBendCompliance,
            Damping = Damping,
            GroundFriction = GroundFriction,
            MaxSpeed = MaxSpeed,
            MaxStretchFraction = MaxStretchFraction
        };
    }
}
