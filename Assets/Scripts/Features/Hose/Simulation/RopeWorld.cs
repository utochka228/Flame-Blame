using System.Collections.Generic;
using UnityEngine;

namespace FlameBlame.Hose.Simulation
{
    public class RopeWorld
    {
        private readonly int _substeps;
        private readonly List<XpbdRope> _ropes = new List<XpbdRope>();
        private readonly List<Junction> _junctions = new List<Junction>();

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

        public void Step(float deltaTime)
        {
            foreach (var rope in _ropes)
                rope.BeginStep(deltaTime, _substeps);

            for (var substep = 0; substep < _substeps; substep++)
            {
                foreach (var rope in _ropes)
                    rope.AdvanceSubstep();

                foreach (var junction in _junctions)
                    SolveJunction(junction);

                foreach (var rope in _ropes)
                    rope.FinishSubstep();
            }

            foreach (var rope in _ropes)
                rope.EndStep();
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
