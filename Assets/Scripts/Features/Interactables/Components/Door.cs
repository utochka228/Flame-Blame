using FlameBlame.Hose.Components;
using FlameBlame.Interactables.Config;
using FlameBlame.Interactables.Simulation;
using UnityEngine;

namespace FlameBlame.Interactables.Components
{
    [RequireComponent(typeof(Rigidbody))]
    public class Door : MonoBehaviour
    {
        [SerializeField] private DoorConfigSO _config;
        [SerializeField] private HoseNetwork _network;
        [SerializeField] private Collider _panel;

        private DoorModel _model;
        private Rigidbody _body;
        private Quaternion _closedRotation;
        private bool _isSlamPending;

        public bool IsOpen => _model.IsOpen;

        private void Awake()
        {
            _model = _config.CreateModel();
            _body = GetComponent<Rigidbody>();
            _body.isKinematic = true;
            _closedRotation = transform.rotation;
        }

        private void OnEnable()
        {
            _model.Slammed += OnSlammed;
        }

        private void OnDisable()
        {
            _model.Slammed -= OnSlammed;
        }

        private void FixedUpdate()
        {
            if (_isSlamPending)
                ApplySlam();

            Move();
        }

        public void Toggle()
        {
            _model.Toggle();
        }

        private void ApplySlam()
        {
            _isSlamPending = false;
            var zone = _panel.bounds;
            zone.Expand(_config.SlamPaddingMeters * 2f);
            _network.Slam(zone);
        }

        private void Move()
        {
            _model.Step(Time.fixedDeltaTime);
            _body.MoveRotation(_closedRotation * Quaternion.AngleAxis(_model.Angle, Vector3.up));
        }

        private void OnSlammed()
        {
            _isSlamPending = true;
        }
    }
}
