using UnityEngine;

namespace FlameBlame.Firefighter.Components
{
    public readonly struct GripAnchor
    {
        public Transform Transform { get; }
        public Vector3 LocalPoint { get; }
        public Rigidbody Body { get; }

        public GripAnchor(Transform transform, Vector3 localPoint, Rigidbody body)
        {
            Transform = transform;
            LocalPoint = localPoint;
            Body = body;
        }

        public Vector3 Position => Transform.TransformPoint(LocalPoint);
    }
}
