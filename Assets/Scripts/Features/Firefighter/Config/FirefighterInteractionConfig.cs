using System;
using UnityEngine;

namespace FlameBlame.Firefighter.Config
{
    [Serializable]
    public class FirefighterInteractionConfig
    {
        [field: SerializeField] [field: Range(1f, 6f)] public float ReachMeters { get; private set; } = 3f;
        [field: SerializeField] [field: Range(0.1f, 1f)] public float AimRadiusMeters { get; private set; } = 0.4f;
        [field: SerializeField] [field: Range(0.2f, 2f)] public float ConnectRadiusMeters { get; private set; } = 0.6f;
        [field: SerializeField] [field: Range(1, 50)] public int ValveStepPercent { get; private set; } = 10;
        [field: SerializeField] public LayerMask Layers { get; private set; } = ~0;
    }
}
