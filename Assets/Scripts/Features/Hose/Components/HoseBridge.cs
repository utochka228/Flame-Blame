using UnityEngine;

namespace FlameBlame.Hose.Components
{
    public class HoseBridge : MonoBehaviour
    {
        private const string BridgeLayerName = "HoseBridge";

        [SerializeField] private HoseSegment _hose;

        private Rigidbody[] _spans;

        private void Awake()
        {
            var config = _hose.Config;
            var layer = LayerMask.NameToLayer(BridgeLayerName);
            Physics.IgnoreLayerCollision(layer, 0);
            _spans = new Rigidbody[config.ParticleCount - 1];
            for (var index = 0; index < _spans.Length; index++)
            {
                var span = new GameObject($"BridgeSpan{index}") { layer = layer };
                span.transform.SetParent(transform, false);
                var capsule = span.AddComponent<CapsuleCollider>();
                capsule.direction = 2;
                capsule.radius = config.BridgeRadiusMeters;
                capsule.height = config.Spacing + capsule.radius * 2f;
                _spans[index] = span.AddComponent<Rigidbody>();
                _spans[index].isKinematic = true;
                span.SetActive(false);
            }
        }

        private void FixedUpdate()
        {
            var positions = _hose.Rope.Positions;
            var pressures = _hose.PressureFractions;
            var threshold = _hose.Config.BridgePressureFraction;
            for (var index = 0; index < _spans.Length; index++)
            {
                var span = _spans[index];
                var isWalkable = Mathf.Min(pressures[index], pressures[index + 1]) >= threshold;
                if (!isWalkable)
                {
                    span.gameObject.SetActive(false);
                    continue;
                }

                var center = (positions[index] + positions[index + 1]) * 0.5f;
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
}
