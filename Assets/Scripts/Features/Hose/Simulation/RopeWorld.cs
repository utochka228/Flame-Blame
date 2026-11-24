using System.Collections.Generic;
using UnityEngine;

namespace FlameBlame.Hose.Simulation
{
    public class RopeWorld
    {
        private readonly int _substeps;
        private readonly List<XpbdRope> _ropes = new List<XpbdRope>();
        private readonly List<Junction> _junctions = new List<Junction>();
        private readonly List<Grip> _grips = new List<Grip>();

        public int JunctionIterations { get; set; } = 4;

        public RopeWorld(int substeps)
        {
            _substeps = substeps;
        }

        public int AddRope(XpbdRope rope)
        {
            _ropes.Add(rope);
            return _ropes.Count - 1;
        }

        public int AddJunction(float breakDistance)
        {
            _junctions.Add(new Junction(breakDistance));
            return _junctions.Count - 1;
        }

        public void Attach(int junction, RopeEnd end)
        {
            _junctions[junction].Ends.Add(end);
        }

        public bool IsIntact(int junction) => !_junctions[junction].IsBroken;

        public Vector3 JunctionPosition(int junction) => JunctionTarget(_junctions[junction]);

        public int AddGrip(RopeParticle particle, float compliance)
        {
            var grip = new Grip(particle, compliance, _ropes[particle.Rope].Positions[particle.Index]);
            var slot = _grips.FindIndex(existing => !existing.IsActive);
            if (slot < 0)
            {
                _grips.Add(grip);
                return _grips.Count - 1;
            }

            _grips[slot] = grip;
            return slot;
        }

        public RopeParticle GripParticle(int grip) => _grips[grip].Particle;

        public void SetGripTarget(int grip, Vector3 target)
        {
            _grips[grip].Target = target;
        }

        public Vector3 GripTension(int grip) => _grips[grip].Tension;

        public void Release(int grip)
        {
            _grips[grip].IsActive = false;
            _grips[grip].Tension = Vector3.zero;
        }

        public void AddImpulse(RopeParticle particle, Vector3 velocityDelta)
        {
            _ropes[particle.Rope].AddVelocity(particle.Index, velocityDelta);
        }

        public RopeParticle FindAlongRay(Vector3 origin, Vector3 direction, float reach, float aimRadius)
        {
            var best = RopeParticle.None;
            var bestOffset = aimRadius;
            for (var rope = 0; rope < _ropes.Count; rope++)
            {
                var positions = _ropes[rope].Positions;
                for (var index = 0; index < positions.Count; index++)
                {
                    if (!_ropes[rope].IsMovable(index)) continue;

                    var toParticle = positions[index] - origin;
                    var along = Vector3.Dot(toParticle, direction);
                    if (along < 0f || along > reach) continue;

                    var offset = (toParticle - direction * along).magnitude;
                    if (offset >= bestOffset) continue;

                    bestOffset = offset;
                    best = new RopeParticle(rope, index);
                }
            }

            return best;
        }

        public void Step(float deltaTime)
        {
            foreach (var rope in _ropes)
                rope.BeginStep(deltaTime, _substeps);

            var substepTime = deltaTime / _substeps;
            for (var substep = 0; substep < _substeps; substep++)
            {
                foreach (var grip in _grips)
                    grip.Displacement = Vector3.zero;

                foreach (var rope in _ropes)
                    rope.AdvanceSubstep();

                for (var iteration = 0; iteration < JunctionIterations; iteration++)
                {
                    foreach (var junction in _junctions)
                        SolveJunction(junction);

                    SolveGrips(substepTime);

                    foreach (var rope in _ropes)
                        rope.SolveDistances();
                }

                foreach (var rope in _ropes)
                    rope.FinishSubstep();

                foreach (var grip in _grips)
                    grip.Tension = grip.IsActive ? -grip.Displacement / (substepTime * substepTime) : Vector3.zero;
            }

            foreach (var rope in _ropes)
                rope.EndStep();
        }

        private void SolveGrips(float substepTime)
        {
            foreach (var grip in _grips)
            {
                var rope = _ropes[grip.Particle.Rope];
                if (!grip.IsActive || !rope.IsMovable(grip.Particle.Index)) continue;

                var alpha = grip.Compliance / (substepTime * substepTime);
                var offset = rope.Positions[grip.Particle.Index] - grip.Target;
                var correction = -offset / (1f + alpha);
                rope.SetPosition(grip.Particle.Index, rope.Positions[grip.Particle.Index] + correction);
                grip.Displacement += correction;
            }
        }

        private void SolveJunction(Junction junction)
        {
            if (junction.IsBroken || junction.Ends.Count == 0) return;

            var target = JunctionTarget(junction);
            if (IsStretchedPastBreakDistance(junction, target))
            {
                junction.IsBroken = true;
                return;
            }

            foreach (var end in junction.Ends)
            {
                var resolved = Resolve(end);
                if (resolved.Rope.IsMovable(resolved.Index))
                    resolved.Rope.SetPosition(resolved.Index, target);
            }
        }

        private bool IsStretchedPastBreakDistance(Junction junction, Vector3 target)
        {
            foreach (var end in junction.Ends)
            {
                if (Vector3.Distance(Resolve(end).Position, target) > junction.BreakDistance)
                    return true;
            }

            return false;
        }

        private Vector3 JunctionTarget(Junction junction)
        {
            var sum = Vector3.zero;
            foreach (var end in junction.Ends)
            {
                var resolved = Resolve(end);
                if (!resolved.Rope.IsMovable(resolved.Index))
                    return resolved.Position;

                sum += resolved.Position;
            }

            return sum / junction.Ends.Count;
        }

        private ResolvedEnd Resolve(RopeEnd end)
        {
            var rope = _ropes[end.Rope];
            var index = end.Kind == RopeEndKind.Start ? 0 : rope.Positions.Count - 1;
            return new ResolvedEnd(rope, index, rope.Positions[index]);
        }

        private readonly struct ResolvedEnd
        {
            public XpbdRope Rope { get; }
            public int Index { get; }
            public Vector3 Position { get; }

            public ResolvedEnd(XpbdRope rope, int index, Vector3 position)
            {
                Rope = rope;
                Index = index;
                Position = position;
            }
        }

        private class Grip
        {
            public RopeParticle Particle { get; }
            public float Compliance { get; }
            public Vector3 Target { get; set; }
            public Vector3 Tension { get; set; }
            public Vector3 Displacement { get; set; }
            public bool IsActive { get; set; } = true;

            public Grip(RopeParticle particle, float compliance, Vector3 target)
            {
                Particle = particle;
                Compliance = compliance;
                Target = target;
            }
        }

        private class Junction
        {
            public float BreakDistance { get; }
            public bool IsBroken { get; set; }
            public List<RopeEnd> Ends { get; } = new List<RopeEnd>();

            public Junction(float breakDistance)
            {
                BreakDistance = breakDistance;
            }
        }
    }
}
