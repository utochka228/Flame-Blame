using System;
using FlameBlame.Hose.Simulation;
using UnityEngine;

namespace FlameBlame.Hose.Config
{
    [Serializable]
    public class HoseRopeConfig
    {
        private static readonly RopeSettings Defaults = new RopeSettings();

        [field: Header("Structure (applied at spawn)")]
        [field: SerializeField] [field: Range(8, 120)] public int ParticleCount { get; private set; } = Defaults.ParticleCount;
        [field: SerializeField] [field: Range(0.1f, 0.4f)] public float Spacing { get; private set; } = Defaults.Spacing;
        [field: SerializeField] [field: Range(0.02f, 0.15f)] public float ParticleRadius { get; private set; } = Defaults.ParticleRadius;
        [field: SerializeField] [field: Range(2, 16)] public int Substeps { get; private set; } = Defaults.Substeps;
        [field: SerializeField] [field: Range(0.1f, 1f)] public float CoilInnerRadius { get; private set; } = 0.25f;
        [field: SerializeField] [field: Range(0f, 0.2f)] public float CoilGap { get; private set; } = 0.05f;

        [field: Header("Stiffness")]
        [field: SerializeField] public float FloppyBendCompliance { get; private set; } = Defaults.FloppyBendCompliance;
        [field: SerializeField] public float RigidBendCompliance { get; private set; } = Defaults.RigidBendCompliance;
        [field: SerializeField] [field: Range(0.02f, 0.5f)] public float MaxStretchFraction { get; private set; } = Defaults.MaxStretchFraction;

        [field: Header("Motion")]
        [field: SerializeField] [field: Range(0f, 5f)] public float Damping { get; private set; } = Defaults.Damping;
        [field: SerializeField] [field: Range(1f, 10f)] public float RigidDampingMultiplier { get; private set; } = 3f;
        [field: SerializeField] [field: Range(0f, 1f)] public float GroundFriction { get; private set; } = Defaults.GroundFriction;
        [field: SerializeField] [field: Range(5f, 40f)] public float MaxSpeed { get; private set; } = Defaults.MaxSpeed;

        [field: Header("Collision")]
        [field: SerializeField] public LayerMask CollisionLayers { get; private set; } = ~0;

        public float Length => (ParticleCount - 1) * Spacing;

        public CoilShape Coil => new CoilShape(CoilInnerRadius, 2f * ParticleRadius + CoilGap, Spacing);

        public void ApplyTo(RopeSettings settings)
        {
            settings.Spacing = Spacing;
            settings.ParticleRadius = ParticleRadius;
            settings.Substeps = Substeps;
            settings.FloppyBendCompliance = FloppyBendCompliance;
            settings.RigidBendCompliance = RigidBendCompliance;
            settings.Damping = Damping;
            settings.RigidDampingMultiplier = RigidDampingMultiplier;
            settings.GroundFriction = GroundFriction;
            settings.MaxSpeed = MaxSpeed;
            settings.MaxStretchFraction = MaxStretchFraction;
        }
    }
}
