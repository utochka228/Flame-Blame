using System;
using UnityEngine;

namespace FlameBlame.Firefighter.Config
{
    [Serializable]
    public class FirefighterLookConfig
    {
        [field: SerializeField] [field: Range(0.01f, 0.5f)] public float DegreesPerPixel { get; private set; } = 0.1f;
        [field: SerializeField] [field: Range(30f, 89f)] public float PitchLimitDegrees { get; private set; } = 85f;
    }
}
