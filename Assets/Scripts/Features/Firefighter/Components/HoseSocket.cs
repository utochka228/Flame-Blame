using FlameBlame.Core.Events;
using FlameBlame.Firefighter.Events;
using FlameBlame.Firefighter.Simulation;
using FlameBlame.Hose.Components;
using FlameBlame.Hose.Simulation;
using UnityEngine;

namespace FlameBlame.Firefighter.Components
{
    public class HoseSocket : MonoBehaviour
    {
        private const int NoGrip = -1;

        [SerializeField] [Min(0.05f)] private float _captureRadiusMeters = 0.5f;
        [SerializeField] private CatchParts _accepts = CatchParts.All;
        [SerializeField] private HoseSplitter _splitter;
        [SerializeField] [Range(0, HoseSplitter.PortCount - 1)] private int _splitterPort;

        private Rigidbody _body;
        private AnchoredGrip _latch;
        private int _aimGrip = NoGrip;
        private Vector3 _aimLocalPoint;

        public HoseSplitter Splitter => _splitter;
        public int SplitterPort => _splitterPort;
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

        public void Aim(RopeWorld world, RopeParticle inner, float spacing, float compliance)
        {
            _aimLocalPoint = Vector3.forward * spacing;
            _aimGrip = world.AddGrip(inner, compliance);
        }

        public void Release(RopeWorld world)
        {
            _latch.Release(world);
            _latch = null;
            if (_aimGrip != NoGrip)
            {
                world.Release(_aimGrip);
                _aimGrip = NoGrip;
            }

            EventBus.Publish(new HoseSocketReleased(Position));
        }

        public bool IsLatchedNear(RopeWorld world, RopeParticle particle, int toleranceParticles)
        {
            return IsOccupied && _latch.IsNear(world, particle, toleranceParticles);
        }

        public void Step(RopeWorld world, float blendSeconds, float deltaTime)
        {
            _latch.Step(world, blendSeconds, deltaTime);
            if (_aimGrip != NoGrip)
                world.SetGripTarget(_aimGrip, transform.TransformPoint(_aimLocalPoint));
        }
    }
}
