using System.Collections.Generic;
using FlameBlame.Water.Config;
using FlameBlame.Water.Simulation;
using UnityEngine;

namespace FlameBlame.Water.Components
{
    public class PuddleField : MonoBehaviour
    {
        private const float RaiseMeters = 0.1f;
        private const float SupportProbeMeters = 0.2f;

        [SerializeField] private PuddleConfigSO _config;

        private PuddleSet _puddles;
        private PuddleSettings _settings;

        public IReadOnlyList<Puddle> Puddles => _puddles.Puddles;
        public int Capacity => _config.Capacity;

        private void Awake()
        {
            _settings = _config.CreateSettings();
            _puddles = new PuddleSet(_settings);
        }

        private void OnEnable()
        {
            _config.Changed += OnConfigChanged;
        }

        private void OnDisable()
        {
            _config.Changed -= OnConfigChanged;
        }

        private void FixedUpdate()
        {
            _puddles.Step(Time.fixedDeltaTime);
            FadeAirborne(Time.fixedDeltaTime);
        }

        public void AddWater(Vector3 point, float volume)
        {
            if (!Physics.Raycast(point + Vector3.up * RaiseMeters, Vector3.down, out var hit, _config.MaxDropMeters + RaiseMeters,
                    _config.GroundLayers, QueryTriggerInteraction.Ignore))
                return;

            _puddles.Add(hit.point, volume);
        }

        private void FadeAirborne(float deltaTime)
        {
            var puddles = _puddles.Puddles;
            for (var index = puddles.Count - 1; index >= 0; index--)
            {
                if (puddles[index].IsFading || !IsSupported(puddles[index].Position))
                    _puddles.Fade(index, deltaTime / _config.AirborneFadeSeconds);
            }
        }

        private bool IsSupported(Vector3 position) =>
            Physics.Raycast(position + Vector3.up * RaiseMeters, Vector3.down, SupportProbeMeters,
                _config.GroundLayers, QueryTriggerInteraction.Ignore);

        public float TractionAt(Vector3 point) => _puddles.TractionAt(point);

        private void OnConfigChanged()
        {
            _config.ApplyTo(_settings);
        }
    }
}
