using FlameBlame.Core.Events;
using FlameBlame.Firefighter.Events;
using FlameBlame.Firefighter.Simulation;
using FlameBlame.Hose.Simulation;
using UnityEngine;

namespace FlameBlame.Firefighter.Components
{
    public class HoseSocket : MonoBehaviour
    {
        [SerializeField] [Min(0.05f)] private float _captureRadiusMeters = 0.5f;
        [SerializeField] private CatchParts _accepts = CatchParts.All;

        private Rigidbody _body;
        private AnchoredGrip _latch;

        public float CaptureRadiusMeters => _captureRadiusMeters;
        public CatchParts Accepts => _accepts;
        public Vector3 Position => transform.position;
        public bool IsOccupied => _latch != null;

        private void Awake()
        {
            _body = GetComponentInParent<Rigidbody>();
        }

        public void Latch(RopeWorld world, RopeParticle particle, float compliance, TieDragSettings dragSettings)
        {
            var anchor = new GripAnchor(transform, Vector3.zero, _body);
            _latch = new AnchoredGrip(world, world.AddGrip(particle, compliance), anchor, dragSettings);
            EventBus.Publish(new HoseSocketLatched(Position));
        }

        public void Release(RopeWorld world)
        {
            _latch.Release(world);
            _latch = null;
            EventBus.Publish(new HoseSocketReleased(Position));
        }

        public bool IsLatchedNear(RopeWorld world, RopeParticle particle, int toleranceParticles)
        {
            return IsOccupied && _latch.IsNear(world, particle, toleranceParticles);
        }

        public void Step(RopeWorld world, float blendSeconds, float deltaTime) => _latch.Step(world, blendSeconds, deltaTime);
    }
}
