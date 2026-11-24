using System.Collections.Generic;
using FlameBlame.Core.Extensions;
using UnityEngine;

namespace FlameBlame.Hose.Simulation
{
    public class RopeJoints
    {
        public const int NoJoint = -1;

        private readonly RopeWorld _world;
        private readonly float _breakDistance;
        private readonly int _maxLiveJoints;
        private readonly List<Joint> _joints = new List<Joint>();
        private readonly List<RopeEnd> _reserved = new List<RopeEnd>();

        public int Count => _joints.Count;

        public RopeJoints(RopeWorld world, float breakDistance, int maxLiveJoints)
        {
            _world = world;
            _breakDistance = breakDistance;
            _maxLiveJoints = maxLiveJoints;
        }

        public void Reserve(RopeEnd end)
        {
            _reserved.Add(end);
        }

        public int Add(IReadOnlyList<RopeEnd> ends)
        {
            if (LiveCount() >= _maxLiveJoints) return NoJoint;

            foreach (var end in ends)
            {
                if (!IsEndFree(end)) return NoJoint;
            }

            var junction = _world.AddJunction(_breakDistance);
            foreach (var end in ends)
                _world.Attach(junction, end);

            var joint = new Joint(ends, junction);
            var slot = _joints.FindIndex(existing => !existing.IsActive);
            if (slot < 0)
            {
                _joints.Add(joint);
                return _joints.Count - 1;
            }

            _joints[slot] = joint;
            return slot;
        }

        public void Remove(int joint)
        {
            _joints[joint].LastPosition = Position(joint);
            _world.RemoveJunction(_joints[joint].Junction);
            _joints[joint].IsActive = false;
        }

        public bool IsLive(int joint) => _joints[joint].IsActive && _world.IsIntact(_joints[joint].Junction);

        public IReadOnlyList<RopeEnd> Ends(int joint) => _joints[joint].Ends;

        public Vector3 Position(int joint)
        {
            return _joints[joint].IsActive ? _world.JunctionPosition(_joints[joint].Junction) : _joints[joint].LastPosition;
        }

        public bool IsEndFree(RopeEnd end)
        {
            foreach (var reserved in _reserved)
            {
                if (IsSameEnd(reserved, end)) return false;
            }

            for (var joint = 0; joint < _joints.Count; joint++)
            {
                if (!IsLive(joint)) continue;

                foreach (var jointEnd in _joints[joint].Ends)
                {
                    if (IsSameEnd(jointEnd, end)) return false;
                }
            }

            return true;
        }

        public int FindAlongRay(Vector3 origin, Vector3 direction, float reach, float aimRadius)
        {
            var best = NoJoint;
            var bestOffset = aimRadius;
            for (var joint = 0; joint < _joints.Count; joint++)
            {
                if (!IsLive(joint)) continue;

                var offset = Position(joint).OffsetFromRay(origin, direction, reach);
                if (offset >= bestOffset) continue;

                bestOffset = offset;
                best = joint;
            }

            return best;
        }

        private static bool IsSameEnd(RopeEnd first, RopeEnd second) => first.Rope == second.Rope && first.Kind == second.Kind;

        private int LiveCount()
        {
            var count = 0;
            for (var joint = 0; joint < _joints.Count; joint++)
            {
                if (IsLive(joint))
                    count++;
            }

            return count;
        }

        private class Joint
        {
            public RopeEnd[] Ends { get; }
            public int Junction { get; }
            public bool IsActive { get; set; } = true;
            public Vector3 LastPosition { get; set; }

            public Joint(IReadOnlyList<RopeEnd> ends, int junction)
            {
                Ends = new RopeEnd[ends.Count];
                for (var index = 0; index < ends.Count; index++)
                    Ends[index] = ends[index];

                Junction = junction;
            }
        }
    }
}
