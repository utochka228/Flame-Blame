using System;
using FlameBlame.Core.Extensions;
using UnityEngine;

namespace FlameBlame.Hose.Config
{
    [Serializable]
    public class HoseTubeConfig
    {
        [field: Header("Mesh (applied at spawn)")]
        [field: SerializeField] [field: Range(1, 8)] public int SamplesPerSegment { get; private set; } = 4;
        [field: SerializeField] [field: Range(6, 24)] public int RingSides { get; private set; } = 12;
        [field: SerializeField] [field: Range(1f, 3f)] public float MiterLimit { get; private set; } = 1.5f;

        [field: Header("Shape")]
        [field: SerializeField] [field: Range(0f, 100f)] public float SwellPercent { get; private set; } = 40f;

        public float SwellFraction => SwellPercent.ToFraction();
    }
}
