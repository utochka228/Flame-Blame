using FlameBlame.Core.Events;
using FlameBlame.Fire.Config;
using FlameBlame.Fire.Events;
using FlameBlame.Fire.Simulation;
using FlameBlame.Hose.Components;
using FlameBlame.Hose.Simulation;
using FlameBlame.Water;
using UnityEngine;

namespace FlameBlame.Fire.Components
{
    public class Fire : MonoBehaviour, IWettable, IHeatSource
    {
        [SerializeField] private FireConfigSO _config;
        [SerializeField] private HeatField _heatField;

        private FireModel _model;
        private FireSettings _settings;
        private Collider _body;

        public float Intensity => _model.Intensity;
        public bool IsOut => _model.IsOut;
        public FireConfigSO Config => _config;

        private void Awake()
        {
            _settings = _config.CreateSettings();
            _model = new FireModel(_settings);
            _body = GetComponent<Collider>();
        }

        private void OnEnable()
        {
            _model.Extinguished += OnExtinguished;
            _config.Changed += OnConfigChanged;
            _heatField.Register(this);
        }

        private void OnDisable()
        {
            _model.Extinguished -= OnExtinguished;
            _config.Changed -= OnConfigChanged;
            _heatField.Unregister(this);
        }

        private void FixedUpdate()
        {
            _model.Tick(Time.fixedDeltaTime);
        }

        public void Soak(float volume)
        {
            _model.AddWater(volume);
        }

        public float HeatAt(Vector3 point) => _model.HeatAt(Vector3.Distance(transform.position, point));

        private void OnConfigChanged()
        {
            _config.ApplyTo(_settings);
        }

        private void OnExtinguished()
        {
            _body.enabled = false;
            EventBus.Publish(new FireExtinguished(transform.position));
        }
    }
}
