using System.Collections.Generic;
using UnityEngine;

namespace FlameBlame.Hose.Simulation
{
    public class PhysicsRopeCollider : IRopeCollider
    {
        private const int MaxOverlaps = 32;
        private const float QueryMargin = 0.5f;
        private const float BoxPushImpulse = 0.02f;

        private readonly Collider[] _candidates = new Collider[MaxOverlaps];
        private readonly LayerMask _layers;
        private readonly SphereCollider _probe;
        private int _candidateCount;

        public PhysicsRopeCollider(LayerMask layers, SphereCollider probe)
        {
            _layers = layers;
            _probe = probe;
        }

        public void Prepare(IReadOnlyList<Vector3> positions, float radius)
        {
            var bounds = new Bounds(positions[0], Vector3.zero);
            for (var index = 1; index < positions.Count; index++)
                bounds.Encapsulate(positions[index]);

            bounds.Expand(2f * (radius + QueryMargin));
            _candidateCount = Physics.OverlapBoxNonAlloc(bounds.center, bounds.extents, _candidates, Quaternion.identity, _layers,
                QueryTriggerInteraction.Ignore);
        }

        public Vector3 Resolve(Vector3 position, float radius)
        {
            _probe.radius = radius;
            for (var index = 0; index < _candidateCount; index++)
            {
                var candidate = _candidates[index];
                if (!Physics.ComputePenetration(_probe, position, Quaternion.identity, candidate, candidate.transform.position,
                        candidate.transform.rotation, out var direction, out var distance))
                    continue;

                position += direction * distance;
                if (candidate.attachedRigidbody && !candidate.attachedRigidbody.isKinematic)
                    candidate.attachedRigidbody.AddForceAtPosition(-direction * (distance * BoxPushImpulse), position, ForceMode.Impulse);
            }

            return position;
        }
    }
}
