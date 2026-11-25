using FlameBlame.Water.Components;
using UnityEngine;

namespace FlameBlame.Water.Presentation
{
    public class PuddleView : MonoBehaviour
    {
        private const float LiftMeters = 0.01f;
        private const float DiameterPerRadius = 2f;

        [SerializeField] private PuddleField _field;
        [SerializeField] private GameObject _discPrefab;

        private Transform[] _discs;

        private void Awake()
        {
            _discs = new Transform[_field.Capacity];
            for (var index = 0; index < _discs.Length; index++)
            {
                _discs[index] = Instantiate(_discPrefab, transform).transform;
                _discs[index].gameObject.SetActive(false);
            }
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
            }
        }
    }
}
