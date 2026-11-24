using System.Collections.Generic;
using FlameBlame.Core.Logging;
using FlameBlame.Hose.Simulation;
using Microsoft.Extensions.Logging;
using UnityEngine;

namespace FlameBlame.Hose.Components
{
    public class PhysicsRopeCollider : IRopeCollider
    {
        private const int MaxOverlaps = 32;
        private const float QueryMargin = 0.5f;

        private static readonly ILogger<PhysicsRopeCollider> Logger = Loggers.For<PhysicsRopeCollider>();

        private readonly Collider[] _candidates = new Collider[MaxOverlaps];
        private readonly LayerMask _layers;
        private readonly SphereCollider _probe;
        private int _candidateCount;
        private bool _hasWarnedAboutOverflow;

        public PhysicsRopeCollider(LayerMask layers, Transform parent)
        {
            _layers = layers;
            var probeObject = new GameObject("HoseCollisionProbe");
            probeObject.transform.SetParent(parent, false);
            _probe = probeObject.AddComponent<SphereCollider>();
            _probe.isTrigger = true;
        }

        public void Prepare(IReadOnlyList<Vector3> positions, float radius)
        {
            var bounds = new Bounds(positions[0], Vector3.zero);
            for (var index = 1; index < positions.Count; index++)
                bounds.Encapsulate(positions[index]);

            bounds.Expand(2f * (radius + QueryMargin));
            _candidateCount = Physics.OverlapBoxNonAlloc(bounds.center, bounds.extents, _candidates, Quaternion.identity, _layers,
                QueryTriggerInteraction.Ignore);

            if (_candidateCount < MaxOverlaps || _hasWarnedAboutOverflow) return;

            _hasWarnedAboutOverflow = true;
            Logger.LogWarning("Hose overlaps {Limit} or more colliders; extra colliders are ignored", MaxOverlaps);
        }

        public Vector3 Resolve(Vector3 position, float radius)
        {
            if (!Mathf.Approximately(_probe.radius, radius))
                _probe.radius = radius;

            for (var index = 0; index < _candidateCount; index++)
            {
                var candidate = _candidates[index];
                if (!Physics.ComputePenetration(_probe, position, Quaternion.identity, candidate, candidate.transform.position,
                        candidate.transform.rotation, out var direction, out var distance))
                    continue;

                position += direction * distance;
            }

            return position;
        }
    }
}
