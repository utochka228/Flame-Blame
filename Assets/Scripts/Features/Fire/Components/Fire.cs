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
        private Collider _body;

        public float Intensity => _model.Intensity;
        public bool IsOut => _model.IsOut;
        public FireConfigSO Config => _config;

        private void Awake()
        {
            _model = new FireModel(_config.CreateSettings());
            _body = GetComponent<Collider>();
        }

        private void OnEnable()
        {
            _model.Extinguished += OnExtinguished;
            _heatField.Register(this);
        }

        private void OnDisable()
        {
            _model.Extinguished -= OnExtinguished;
            _heatField.Unregister(this);
        }

        public void Soak(float volume)
        {
            _model.Douse(volume);
        }

        public float HeatAt(Vector3 point) => _model.HeatAt(Vector3.Distance(transform.position, point));

        private void OnExtinguished()
        {
            _body.enabled = false;
            EventBus.Publish(new FireExtinguished(transform.position));
        }
    }
}
