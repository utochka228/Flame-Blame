using FlameBlame.Hose.Components;
using FlameBlame.Interactables.Config;
using FlameBlame.Interactables.Simulation;
using UnityEngine;

namespace FlameBlame.Interactables.Components
{
    [DefaultExecutionOrder(-10)]
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
        private float _fromAngle;
        private float _toAngle;

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
            _network.SubstepStarting += OnSubstepStarting;
        }

        private void OnDisable()
        {
            _model.Slammed -= OnSlammed;
            _network.SubstepStarting -= OnSubstepStarting;
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
            _fromAngle = _toAngle;
            _model.Step(Time.fixedDeltaTime);
            _toAngle = _model.Angle;
            _body.MoveRotation(RotationAt(_toAngle));
        }

        private Quaternion RotationAt(float angle) => _closedRotation * Quaternion.AngleAxis(angle, Vector3.up);

        private void OnSubstepStarting(float fraction)
        {
            if (Mathf.Approximately(_fromAngle, _toAngle)) return;

            transform.rotation = RotationAt(Mathf.Lerp(_fromAngle, _toAngle, fraction));
            Physics.SyncTransforms();
        }

        private void OnSlammed()
        {
            _isSlamPending = true;
        }
    }
}
