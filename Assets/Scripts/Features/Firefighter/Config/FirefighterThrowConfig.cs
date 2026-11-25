using System;
using UnityEngine;

namespace FlameBlame.Firefighter.Config
{
    [Serializable]
    public class FirefighterThrowConfig
    {
        [field: SerializeField] [field: Range(1f, 30f)] public float Speed { get; private set; } = 12f;
        [field: SerializeField] [field: Range(0f, 2f)] public float InheritMultiplier { get; private set; } = 1f;
        [field: SerializeField] [field: Range(0, 20)] public int SpreadParticles { get; private set; } = 6;
    }
}
