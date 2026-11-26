using FlameBlame.Firefighter.Simulation;
using FlameBlame.Hose.Simulation;
using UnityEngine;

namespace FlameBlame.Firefighter.Components
{
    public class AnchoredGrip
    {
        private readonly int _grip;
        private readonly GripAnchor _anchor;
        private readonly Vector3 _startPosition;
        private readonly TieDrag _drag;
        private float _blend;

        public Rigidbody Body => _anchor.Body;

        public AnchoredGrip(RopeWorld world, int grip, GripAnchor anchor, TieDragSettings dragSettings)
        {
            _grip = grip;
            _anchor = anchor;
            _startPosition = world.ParticlePosition(world.GripParticle(grip));
            _drag = new TieDrag(dragSettings);
        }

        public void Step(RopeWorld world, float blendSeconds, float deltaTime)
        {
            _blend = Mathf.MoveTowards(_blend, 1f, deltaTime / blendSeconds);
            var anchor = _anchor.Position;
            world.SetGripTarget(_grip, Vector3.Lerp(_startPosition, anchor, _blend));
            if (_blend < 1f || !Body || Body.isKinematic) return;

            var force = _drag.Step(world.GripTension(_grip), world.IsGripShared(_grip), Body.linearVelocity, Body.mass, deltaTime);
            Body.AddForceAtPosition(force, anchor);
        }

        public bool IsNear(RopeWorld world, RopeParticle particle, int toleranceParticles)
        {
            var held = world.GripParticle(_grip);
            return held.Rope == particle.Rope && Mathf.Abs(held.Index - particle.Index) <= toleranceParticles;
        }

        public void Release(RopeWorld world) => world.Release(_grip);
    }
}
