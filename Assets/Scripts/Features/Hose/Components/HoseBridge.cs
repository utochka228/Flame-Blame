using UnityEngine;

namespace FlameBlame.Hose.Components
{
    public class HoseBridge : MonoBehaviour
    {
        private const string BridgeLayerName = "HoseBridge";
        private const int CapsuleAlongZ = 2;

        [SerializeField] private HoseSegment _hose;

        private Rigidbody[] _spans;
        private Vector3[] _lastCenters;
        private bool[] _hasLastCenter;

        private void Awake()
        {
            var layer = LayerMask.NameToLayer(BridgeLayerName);
            Physics.IgnoreLayerCollision(layer, 0);
            _spans = new Rigidbody[_hose.Config.ParticleCount - 1];
            _lastCenters = new Vector3[_spans.Length];
            _hasLastCenter = new bool[_spans.Length];
            for (var index = 0; index < _spans.Length; index++)
                _spans[index] = CreateSpan(index, layer);
        }

        private void FixedUpdate()
        {
            var threshold = _hose.Config.BridgePressureFraction;
            for (var index = 0; index < _spans.Length; index++)
            {
                var isWalkable = Mathf.Min(_hose.PressureFractions[index], _hose.PressureFractions[index + 1]) >= threshold;
                if (isWalkable && IsSettled(index))
                    PlaceSpan(index);
                else
                {
                    _hasLastCenter[index] = false;
                    _spans[index].gameObject.SetActive(false);
                }
            }
        }

        private Rigidbody CreateSpan(int index, int layer)
        {
            var config = _hose.Config;
            var span = new GameObject($"BridgeSpan{index}") { layer = layer };
            span.transform.SetParent(transform, false);
            var capsule = span.AddComponent<CapsuleCollider>();
            capsule.direction = CapsuleAlongZ;
            capsule.radius = config.BridgeRadiusMeters;
            capsule.height = config.Spacing + capsule.radius * 2f;
            var body = span.AddComponent<Rigidbody>();
            body.isKinematic = true;
            span.SetActive(false);
            return body;
        }

        private bool IsSettled(int index)
        {
            var center = CenterOf(index);
            var speed = _hasLastCenter[index] ? (center - _lastCenters[index]).magnitude / Time.fixedDeltaTime : 0f;
            _lastCenters[index] = center;
            _hasLastCenter[index] = true;
            return speed <= _hose.Config.BridgeMaxSpanSpeed;
        }

        private Vector3 CenterOf(int index)
        {
            var positions = _hose.Rope.Positions;
            return (positions[index] + positions[index + 1]) * 0.5f;
        }

        private void PlaceSpan(int index)
        {
            var positions = _hose.Rope.Positions;
            var span = _spans[index];
            var center = CenterOf(index);
            var rotation = Quaternion.LookRotation(positions[index + 1] - positions[index]);
            if (!span.gameObject.activeSelf)
            {
                span.transform.SetPositionAndRotation(center, rotation);
                span.gameObject.SetActive(true);
            }

            span.MovePosition(center);
            span.MoveRotation(rotation);
        }
    }
}
