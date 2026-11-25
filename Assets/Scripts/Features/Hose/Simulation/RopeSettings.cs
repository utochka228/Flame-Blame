using UnityEngine;

namespace FlameBlame.Hose.Simulation
{
    public class RopeSettings
    {
        public int ParticleCount { get; set; } = 50;
        public float Spacing { get; set; } = 0.2f;
        public float ParticleRadius { get; set; } = 0.05f;
        public int Substeps { get; set; } = 8;
        public float FloppyBendCompliance { get; set; } = 1f;
        public float RigidBendCompliance { get; set; } = 1e-6f;
        public float Damping { get; set; } = 2.5f;
        public float RigidDampingMultiplier { get; set; } = 1f;
        public float GroundFriction { get; set; } = 0.3f;
        public float MaxSpeed { get; set; } = 20f;
        public float MaxStretchFraction { get; set; } = 0.1f;
        public Vector3 Gravity { get; set; } = new Vector3(0f, -9.81f, 0f);
    }
}
