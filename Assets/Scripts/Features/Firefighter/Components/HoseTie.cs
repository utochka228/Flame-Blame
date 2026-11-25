using System.Collections.Generic;
using FlameBlame.Firefighter.Config;
using FlameBlame.Firefighter.Simulation;
using FlameBlame.Hose.Components;
using FlameBlame.Hose.Simulation;
using UnityEngine;

namespace FlameBlame.Firefighter.Components
{
    public class HoseTie : MonoBehaviour
    {
        private const float SurfaceClearanceMultiplier = 1.2f;

        [SerializeField] private FirefighterConfigSO _config;
        [SerializeField] private HoseNetwork _network;

        private readonly List<TieBinding> _ties = new List<TieBinding>();

        private void FixedUpdate()
        {
            foreach (var tie in _ties)
            {
                tie.Blend = Mathf.MoveTowards(tie.Blend, 1f, Time.fixedDeltaTime / _config.Tie.BlendSeconds);
                var anchor = tie.Body.transform.TransformPoint(tie.LocalAnchor);
                _network.World.SetGripTarget(tie.Grip, Vector3.Lerp(tie.StartPosition, anchor, tie.Blend));
                if (tie.Blend < 1f) continue;

                var isPulled = _network.World.IsGripShared(tie.Grip);
                var force = tie.Drag.Step(_network.World.GripTension(tie.Grip), isPulled, tie.Body.linearVelocity, tie.Body.mass, Time.fixedDeltaTime);
                tie.Body.AddForceAtPosition(force, anchor);
            }
        }

        public bool TryFindTarget(Transform eye, out TieTarget target)
        {
            target = default;
            if (!Physics.Raycast(eye.position, eye.forward, out var hit, _config.Interaction.ReachMeters, _config.Interaction.Layers, QueryTriggerInteraction.Ignore))
                return false;

            if (!hit.rigidbody || hit.rigidbody.isKinematic) return false;

            target = new TieTarget(hit.rigidbody, hit.point, hit.normal);
            return true;
        }

        public bool IsTied(Rigidbody body) => _ties.Exists(tie => tie.Body == body);

        public void Tie(RopeParticle particle, TieTarget target)
        {
            var grip = _network.World.AddGrip(particle, _config.Grip.Compliance);
            var start = _network.World.ParticlePosition(particle);
            var clearance = _network.HoseOf(particle.Rope).Config.Rope.ParticleRadius * SurfaceClearanceMultiplier;
            var anchor = target.Body.transform.InverseTransformPoint(target.Point + target.Normal * clearance);
            _ties.Add(new TieBinding(grip, target.Body, anchor, start, new TieDrag(_config.Tie.CreateDragSettings())));
        }

        public bool UntieAt(RopeParticle particle, int toleranceParticles)
        {
            var tie = _ties.Find(binding =>
            {
                var tied = _network.World.GripParticle(binding.Grip);
                return tied.Rope == particle.Rope && Mathf.Abs(tied.Index - particle.Index) <= toleranceParticles;
            });
            if (tie == null) return false;

            Untie(tie.Body);
            return true;
        }

        public void Untie(Rigidbody body)
        {
            foreach (var tie in _ties.FindAll(binding => binding.Body == body))
                _network.World.Release(tie.Grip);

            _ties.RemoveAll(binding => binding.Body == body);
        }

        public readonly struct TieTarget
        {
            public Rigidbody Body { get; }
            public Vector3 Point { get; }
            public Vector3 Normal { get; }

            public TieTarget(Rigidbody body, Vector3 point, Vector3 normal)
            {
                Body = body;
                Point = point;
                Normal = normal;
            }
        }

        private class TieBinding
        {
            public int Grip { get; }
            public Rigidbody Body { get; }
            public Vector3 LocalAnchor { get; }
            public Vector3 StartPosition { get; }
            public TieDrag Drag { get; }
            public float Blend { get; set; }

            public TieBinding(int grip, Rigidbody body, Vector3 localAnchor, Vector3 startPosition, TieDrag drag)
            {
                Drag = drag;
                Grip = grip;
                Body = body;
                LocalAnchor = localAnchor;
                StartPosition = startPosition;
            }
        }
    }
}
