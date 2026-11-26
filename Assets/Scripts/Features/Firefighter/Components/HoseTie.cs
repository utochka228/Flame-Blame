using System.Collections.Generic;
using FlameBlame.Firefighter.Config;
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

        private readonly List<AnchoredGrip> _ties = new List<AnchoredGrip>();

        private void FixedUpdate()
        {
            foreach (var tie in _ties)
                tie.Step(_network.World, _config.Tie.BlendSeconds, Time.fixedDeltaTime);
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
            var clearance = _network.HoseOf(particle.Rope).Config.Rope.ParticleRadius * SurfaceClearanceMultiplier;
            var bodyTransform = target.Body.transform;
            var anchor = new GripAnchor(bodyTransform, bodyTransform.InverseTransformPoint(target.Point + target.Normal * clearance), target.Body);
            _ties.Add(new AnchoredGrip(_network.World, grip, anchor, _config.Tie.CreateDragSettings()));
        }

        public bool UntieAt(RopeParticle particle, int toleranceParticles)
        {
            var tie = _ties.Find(binding => binding.IsNear(_network.World, particle, toleranceParticles));
            if (tie == null) return false;

            Untie(tie.Body);
            return true;
        }

        public void Untie(Rigidbody body)
        {
            foreach (var tie in _ties.FindAll(binding => binding.Body == body))
                tie.Release(_network.World);

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
    }
}
