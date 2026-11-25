using System;
using FlameBlame.Firefighter.Simulation;
using UnityEngine;

namespace FlameBlame.Firefighter.Config
{
    [Serializable]
    public class FirefighterTieConfig
    {
        [field: Header("Tie")]
        [field: SerializeField] [field: Range(0.1f, 3f)] public float BlendSeconds { get; private set; } = 0.6f;
        [field: SerializeField] [field: Range(0, 5)] public int GrabToleranceParticles { get; private set; } = 2;

        [field: Header("Drag")]
        [field: SerializeField] [field: Range(0f, 5000f)] public float MinTension { get; private set; } = 500f;
        [field: SerializeField] [field: Range(0f, 1f)] public float ForceMultiplier { get; private set; } = 0.04f;
        [field: SerializeField] [field: Range(10f, 2000f)] public float MaxForce { get; private set; } = 300f;
        [field: SerializeField] [field: Range(0.5f, 10f)] public float MaxSpeed { get; private set; } = 2f;
        [field: SerializeField] [field: Range(0.05f, 2f)] public float BrakeSeconds { get; private set; } = 0.25f;

        public TieDragSettings CreateDragSettings() => new TieDragSettings
        {
            MinTension = MinTension,
            ForceMultiplier = ForceMultiplier,
            MaxForce = MaxForce,
            MaxSpeed = MaxSpeed,
            BrakeSeconds = BrakeSeconds
        };
    }
}
