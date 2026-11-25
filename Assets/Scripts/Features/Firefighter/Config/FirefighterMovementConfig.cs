using System;
using FlameBlame.Core.Extensions;
using UnityEngine;

namespace FlameBlame.Firefighter.Config
{
    [Serializable]
    public class FirefighterMovementConfig
    {
        [field: Header("Ground")]
        [field: SerializeField] [field: Range(1f, 10f)] public float MoveSpeed { get; private set; } = 5f;
        [field: SerializeField] [field: Range(1f, 100f)] public float Acceleration { get; private set; } = 40f;
        [field: SerializeField] [field: Range(1f, 100f)] public float Deceleration { get; private set; } = 10f;
        [field: SerializeField] [field: Range(5f, 40f)] public float MaxSpeed { get; private set; } = 20f;

        [field: Header("Air")]
        [field: SerializeField] [field: Range(1f, 15f)] public float JumpSpeed { get; private set; } = 6f;
        [field: SerializeField] [field: Range(0f, 100f)] public float AirControlPercent { get; private set; } = 15f;

        [field: Header("Ground check")]
        [field: SerializeField] [field: Range(0.01f, 0.5f)] public float GroundCheckDistance { get; private set; } = 0.1f;
        [field: SerializeField] public LayerMask GroundLayers { get; private set; } = ~0;

        public float AirControlFraction => AirControlPercent.ToFraction();
    }
}
