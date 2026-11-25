using System;
using UnityEngine;

namespace FlameBlame.Firefighter.Config
{
    [Serializable]
    public class FirefighterGripConfig
    {
        [field: Header("Hold")]
        [field: SerializeField] [field: Range(0.5f, 3f)] public float HoldDistanceMeters { get; private set; } = 1.5f;
        [field: SerializeField] [field: Range(1f, 10f)] public float MaxDistanceMeters { get; private set; } = 3.5f;
        [field: SerializeField] [field: Range(0.01f, 0.5f)] public float FollowSeconds { get; private set; } = 0.12f;
        [field: SerializeField] [field: Min(1e-9f)] public float Compliance { get; private set; } = 1e-7f;
        [field: SerializeField] [field: Min(1e-9f)] public float AimCompliance { get; private set; } = 1e-5f;

        [field: Header("Pull and recoil")]
        [field: SerializeField] [field: Range(0f, 0.5f)] public float PullAccelerationMultiplier { get; private set; } = 0.05f;
        [field: SerializeField] [field: Range(1f, 30f)] public float MaxPullAcceleration { get; private set; } = 6f;
        [field: SerializeField] [field: Range(0.02f, 0.5f)] public float PullSmoothingSeconds { get; private set; } = 0.15f;
        [field: SerializeField] [field: Range(0f, 1f)] public float RecoilTransferMultiplier { get; private set; } = 1f;
    }
}
