using System.Collections.Generic;
using FlameBlame.Hose.Components;
using UnityEngine;

namespace FlameBlame.Hose.Presentation
{
    public class HoseEndCaps : MonoBehaviour
    {
        [SerializeField] private HoseSegment _hose;
        [SerializeField] private Transform _capPrefab;
        [SerializeField] private Vector3 _rotationOffset = new Vector3(90f, 0f, 0f);
        [SerializeField] [Min(0.001f)] private float _modelRadiusMeters = 0.09f;

        private readonly List<OpenEnd> _ends = new List<OpenEnd>();
        private readonly List<Transform> _caps = new List<Transform>();

        private void Start()
        {
            var nozzle = _hose.GetComponent<Nozzle>();
            foreach (var end in _hose.OpenEnds)
            {
                if (nozzle && nozzle.TipEnd == end.Kind) continue;

                _ends.Add(end);
                _caps.Add(Instantiate(_capPrefab, transform));
            }
        }

        private void LateUpdate()
        {
            var positions = _hose.Positions;
            var config = _hose.Config;
            for (var index = 0; index < _caps.Count; index++)
            {
                var end = _ends[index];
                var tip = positions[end.ParticleIndex];
                var outward = tip - positions[end.InnerIndex];
                var radius = HoseRadiusProfile.SwollenRadius(config.Rope.ParticleRadius, config.Tube.SwellFraction, _hose.PressureFractions[end.ParticleIndex]);
                _caps[index].SetPositionAndRotation(tip, Quaternion.LookRotation(outward) * Quaternion.Euler(_rotationOffset));
                _caps[index].localScale = Vector3.one * (radius / _modelRadiusMeters);
            }
        }
    }
}
