using System;
using UnityEngine;

namespace FlameBlame.Hose.Config
{
    [Serializable]
    public class HoseColorStyle
    {
        [field: Header("Pressure tint")]
        [field: SerializeField] public Color RelaxedColor { get; private set; } = new Color(0.9f, 0.2f, 0.15f);
        [field: SerializeField] public Color StrainedColor { get; private set; } = new Color(1f, 0.6f, 0.1f);

        [field: Header("Breach paint")]
        [field: SerializeField] public Color BurnedColor { get; private set; } = new Color(0.06f, 0.05f, 0.05f);
        [field: SerializeField] public Color TapedColor { get; private set; } = new Color(0.78f, 0.8f, 0.84f);
        [field: SerializeField] [field: Range(0f, 3f)] public float BreachFeatherParticles { get; private set; } = 0.6f;
    }
}
