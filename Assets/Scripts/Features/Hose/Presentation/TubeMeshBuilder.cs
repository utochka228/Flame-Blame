using System.Collections.Generic;
using UnityEngine;

namespace FlameBlame.Hose.Presentation
{
    public class TubeMeshBuilder
    {
        private const float MinLength = 1e-4f;

        private readonly int _samplesPerSegment;
        private readonly int _ringSides;
        private readonly float _miterLimit;
        private readonly Vector3[] _centers;
        private readonly float[] _uniformRadii;

        public Vector3[] Vertices { get; }
        public Vector3[] Normals { get; }
        public int[] Triangles { get; }
        public IReadOnlyList<Vector3> Centers => _centers;

        public TubeMeshBuilder(int particleCount, int samplesPerSegment, int ringSides, float miterLimit)
        {
            _samplesPerSegment = samplesPerSegment;
            _ringSides = ringSides;
            _miterLimit = miterLimit;
            _uniformRadii = new float[particleCount];

            var ringCount = (particleCount - 1) * samplesPerSegment + 1;
            _centers = new Vector3[ringCount];
            Vertices = new Vector3[ringCount * ringSides];
            Normals = new Vector3[Vertices.Length];
            Triangles = BuildTriangles(ringCount, ringSides);
        }

        public void Build(IReadOnlyList<Vector3> points, float radius)
        {
            for (var index = 0; index < _uniformRadii.Length; index++)
                _uniformRadii[index] = radius;

            Build(points, _uniformRadii);
        }

        public void Build(IReadOnlyList<Vector3> points, IReadOnlyList<float> radii)
        {
            SampleCenters(points);
            BuildRings(radii);
        }

        private static int[] BuildTriangles(int ringCount, int ringSides)
        {
            var triangles = new int[(ringCount - 1) * ringSides * 6];
            var cursor = 0;
            for (var ring = 0; ring < ringCount - 1; ring++)
            {
                for (var side = 0; side < ringSides; side++)
                {
                    var current = ring * ringSides + side;
                    var wrapped = ring * ringSides + (side + 1) % ringSides;
                    triangles[cursor++] = current;
                    triangles[cursor++] = wrapped;
                    triangles[cursor++] = current + ringSides;
                    triangles[cursor++] = wrapped;
                    triangles[cursor++] = wrapped + ringSides;
                    triangles[cursor++] = current + ringSides;
                }
            }

            return triangles;
        }

        private static Vector3 CentripetalCatmullRom(Vector3 before, Vector3 start, Vector3 end, Vector3 after, float fraction)
        {
            var beforeKnot = 0f;
            var startKnot = beforeKnot + KnotInterval(before, start);
            var endKnot = startKnot + KnotInterval(start, end);
            var afterKnot = endKnot + KnotInterval(end, after);
            var knot = Mathf.Lerp(startKnot, endKnot, fraction);

            var beforeToStart = Blend(before, start, knot, beforeKnot, startKnot);
            var startToEnd = Blend(start, end, knot, startKnot, endKnot);
            var endToAfter = Blend(end, after, knot, endKnot, afterKnot);
            var firstPair = Blend(beforeToStart, startToEnd, knot, beforeKnot, endKnot);
            var secondPair = Blend(startToEnd, endToAfter, knot, startKnot, afterKnot);
            return Blend(firstPair, secondPair, knot, startKnot, endKnot);
        }

        private static Vector3 Blend(Vector3 from, Vector3 to, float knot, float fromKnot, float toKnot)
        {
            return Vector3.LerpUnclamped(from, to, (knot - fromKnot) / (toKnot - fromKnot));
        }

        private static float KnotInterval(Vector3 from, Vector3 to) => Mathf.Sqrt(Mathf.Max(Vector3.Distance(from, to), MinLength));

        private static Vector3 AnyPerpendicular(Vector3 tangent)
        {
            return Vector3.Cross(tangent, Mathf.Abs(tangent.y) > 0.99f ? Vector3.right : Vector3.up).normalized;
        }

        private static bool IsUsable(Vector3 vector) => vector.sqrMagnitude > MinLength * MinLength;

        private void SampleCenters(IReadOnlyList<Vector3> points)
        {
            var last = points.Count - 1;
            for (var segment = 0; segment < last; segment++)
            {
                var before = points[Mathf.Max(segment - 1, 0)];
                var start = points[segment];
                var end = points[segment + 1];
                var after = points[Mathf.Min(segment + 2, last)];
                for (var sample = 0; sample < _samplesPerSegment; sample++)
                    _centers[segment * _samplesPerSegment + sample] =
                        CentripetalCatmullRom(before, start, end, after, (float)sample / _samplesPerSegment);
            }

            _centers[_centers.Length - 1] = points[last];
        }

        private float RingRadius(IReadOnlyList<float> radii, int ring)
        {
            var segment = ring / _samplesPerSegment;
            var next = Mathf.Min(segment + 1, radii.Count - 1);
            return Mathf.Lerp(radii[segment], radii[next], (float)(ring % _samplesPerSegment) / _samplesPerSegment);
        }

        private void BuildRings(IReadOnlyList<float> radii)
        {
            var tangent = (_centers[1] - _centers[0]).normalized;
            var side = AnyPerpendicular(tangent);
            for (var ring = 0; ring < _centers.Length; ring++)
            {
                var incoming = _centers[ring] - _centers[Mathf.Max(ring - 1, 0)];
                var outgoing = _centers[Mathf.Min(ring + 1, _centers.Length - 1)] - _centers[ring];
                if (IsUsable(incoming + outgoing))
                    tangent = (incoming + outgoing).normalized;

                var projectedSide = Vector3.ProjectOnPlane(side, tangent);
                side = IsUsable(projectedSide) ? projectedSide.normalized : AnyPerpendicular(tangent);
                var up = Vector3.Cross(tangent, side);

                var radius = RingRadius(radii, ring);
                var bendAxis = BendAxis(incoming, outgoing, tangent, out var miterScale);
                for (var vertex = 0; vertex < _ringSides; vertex++)
                {
                    var angle = vertex * Mathf.PI * 2f / _ringSides;
                    var normal = side * Mathf.Cos(angle) + up * Mathf.Sin(angle);
                    var offset = normal * radius;
                    offset += bendAxis * (Vector3.Dot(offset, bendAxis) * (miterScale - 1f));

                    var index = ring * _ringSides + vertex;
                    Normals[index] = normal;
                    Vertices[index] = _centers[ring] + offset;
                }
            }
        }

        private Vector3 BendAxis(Vector3 incoming, Vector3 outgoing, Vector3 tangent, out float miterScale)
        {
            miterScale = 1f;
            if (!IsUsable(incoming) || !IsUsable(outgoing)) return Vector3.zero;

            var incomingDirection = incoming.normalized;
            var bend = Vector3.ProjectOnPlane(outgoing.normalized - incomingDirection, tangent);
            if (!IsUsable(bend)) return Vector3.zero;

            var alignment = Mathf.Max(Vector3.Dot(tangent, incomingDirection), 1f / _miterLimit);
            miterScale = Mathf.Min(1f / alignment, _miterLimit);
            return bend.normalized;
        }
    }
}
