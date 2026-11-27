using FlameBlame.Water.Components;
using UnityEngine;

namespace FlameBlame.Water.Presentation
{
    public class PuddleView : MonoBehaviour
    {
        private const float LiftMeters = 0.01f;
        private const float DiameterPerRadius = 2f;
        private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");

        [SerializeField] private PuddleField _field;
        [SerializeField] private GameObject _discPrefab;

        private Transform[] _discs;
        private Renderer[] _renderers;
        private float[] _opacities;
        private MaterialPropertyBlock _block;
        private Color _baseColor;

        private void Awake()
        {
            _discs = new Transform[_field.Capacity];
            _renderers = new Renderer[_field.Capacity];
            _opacities = new float[_field.Capacity];
            _block = new MaterialPropertyBlock();
            for (var index = 0; index < _discs.Length; index++)
            {
                _discs[index] = Instantiate(_discPrefab, transform).transform;
                _renderers[index] = _discs[index].GetComponentInChildren<Renderer>();
                _opacities[index] = 1f;
                _discs[index].gameObject.SetActive(false);
            }

            _baseColor = _renderers[0].sharedMaterial.GetColor(BaseColorId);
        }

        private void LateUpdate()
        {
            var puddles = _field.Puddles;
            for (var index = 0; index < _discs.Length; index++)
            {
                var isUsed = index < puddles.Count;
                _discs[index].gameObject.SetActive(isUsed);
                if (!isUsed) continue;

                var diameter = puddles[index].Radius * DiameterPerRadius;
                _discs[index].SetPositionAndRotation(puddles[index].Position + Vector3.up * LiftMeters, Quaternion.identity);
                _discs[index].localScale = new Vector3(diameter, 1f, diameter);

                ShowOpacity(index, puddles[index].Opacity);
            }
        }

        private void ShowOpacity(int index, float opacity)
        {
            if (opacity == _opacities[index]) return;

            _opacities[index] = opacity;
            if (opacity >= 1f)
            {
                _renderers[index].SetPropertyBlock(null);
                return;
            }

            var color = _baseColor;
            color.a *= opacity;
            _block.SetColor(BaseColorId, color);
            _renderers[index].SetPropertyBlock(_block);
        }
    }
}
