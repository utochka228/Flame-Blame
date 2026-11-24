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
                {
                    rope.Integrate();
                    rope.SolveConstraints();
                }

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
            foreach (var end in junction.Ends)
            {
                if (Vector3.Distance(EndPosition(end), target) > junction.BreakDistance)
                {
                    junction.IsBroken = true;
                    return;
                }
            }

            foreach (var end in junction.Ends)
            {
                if (_ropes[end.Rope].IsMovable(EndIndex(end)))
                    _ropes[end.Rope].SetPosition(EndIndex(end), target);
            }
        }

        private Vector3 JunctionTarget(Junction junction)
        {
            var sum = Vector3.zero;
            foreach (var end in junction.Ends)
            {
                if (!_ropes[end.Rope].IsMovable(EndIndex(end)))
                    return EndPosition(end);

                sum += EndPosition(end);
            }

            return sum / junction.Ends.Count;
        }

        private int EndIndex(RopeEnd end) =>
            end.Kind == RopeEndKind.Start ? 0 : _ropes[end.Rope].Positions.Count - 1;

        private Vector3 EndPosition(RopeEnd end) => _ropes[end.Rope].Positions[EndIndex(end)];

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
