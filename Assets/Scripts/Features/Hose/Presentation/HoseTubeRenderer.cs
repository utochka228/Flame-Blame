using FlameBlame.Hose.Components;
using UnityEngine;

namespace FlameBlame.Hose.Presentation
{
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
    public class HoseTubeRenderer : MonoBehaviour
    {
        private const float MinFrameLength = 1e-4f;
        private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");

        [SerializeField] private HoseSegment _hose;

        private Mesh _mesh;
        private MeshRenderer _meshRenderer;
        private MaterialPropertyBlock _propertyBlock;
        private Vector3[] _centers;
        private Vector3[] _vertices;
        private Vector3[] _normals;
        private int _ringSides;
        private int _samplesPerSegment;

        private void Awake()
        {
            _meshRenderer = GetComponent<MeshRenderer>();
            _propertyBlock = new MaterialPropertyBlock();
            _mesh = new Mesh { name = "HoseTube" };
            _mesh.MarkDynamic();
            GetComponent<MeshFilter>().sharedMesh = _mesh;
            transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
        }

        private void Start()
        {
            _ringSides = _hose.Config.TubeRingSides;
            _samplesPerSegment = _hose.Config.TubeSamplesPerSegment;
            var ringCount = (_hose.Positions.Count - 1) * _samplesPerSegment + 1;
            _centers = new Vector3[ringCount];
            _vertices = new Vector3[ringCount * _ringSides];
            _normals = new Vector3[_vertices.Length];
            _mesh.vertices = _vertices;
            _mesh.triangles = BuildTriangles(ringCount, _ringSides);
        }

        private void LateUpdate()
        {
            SampleCenters();
            var radius = _hose.Config.ParticleRadius * (1f + _hose.Config.SwellFraction * _hose.PressureFraction);
            BuildRings(radius);
            _mesh.vertices = _vertices;
            _mesh.normals = _normals;
            _mesh.RecalculateBounds();

            _propertyBlock.SetColor(BaseColorId, Color.Lerp(_hose.Config.RelaxedColor, _hose.Config.StrainedColor, _hose.PressureFraction));
            _meshRenderer.SetPropertyBlock(_propertyBlock);
        }

        private void OnDestroy()
        {
            Destroy(_mesh);
        }

        private void SampleCenters()
        {
            var points = _hose.Positions;
            var last = points.Count - 1;
            for (var segment = 0; segment < last; segment++)
            {
                var before = points[Mathf.Max(segment - 1, 0)];
                var start = points[segment];
                var end = points[segment + 1];
                var after = points[Mathf.Min(segment + 2, last)];
                for (var sample = 0; sample < _samplesPerSegment; sample++)
                    _centers[segment * _samplesPerSegment + sample] = CatmullRom(before, start, end, after, (float)sample / _samplesPerSegment);
            }

            _centers[_centers.Length - 1] = points[last];
        }

        private void BuildRings(float radius)
        {
            var miterLimit = _hose.Config.TubeMiterLimit;
            var tangent = (_centers[1] - _centers[0]).normalized;
            var side = Vector3.Cross(tangent, Mathf.Abs(tangent.y) > 0.99f ? Vector3.right : Vector3.up).normalized;
            for (var ring = 0; ring < _centers.Length; ring++)
            {
                var incoming = _centers[ring] - _centers[Mathf.Max(ring - 1, 0)];
                var outgoing = _centers[Mathf.Min(ring + 1, _centers.Length - 1)] - _centers[ring];
                var span = incoming + outgoing;
                if (span.sqrMagnitude > MinFrameLength * MinFrameLength)
                    tangent = span.normalized;

                var projectedSide = Vector3.ProjectOnPlane(side, tangent);
                side = projectedSide.sqrMagnitude > MinFrameLength * MinFrameLength
                    ? projectedSide.normalized
                    : Vector3.Cross(tangent, Mathf.Abs(tangent.y) > 0.99f ? Vector3.right : Vector3.up).normalized;
                var up = Vector3.Cross(tangent, side);

                var bendAxis = Vector3.ProjectOnPlane(outgoing.normalized - incoming.normalized, tangent);
                var hasBend = bendAxis.sqrMagnitude > MinFrameLength * MinFrameLength && incoming.sqrMagnitude > 0f && outgoing.sqrMagnitude > 0f;
                var miterScale = 1f;
                if (hasBend)
                {
                    bendAxis.Normalize();
                    miterScale = Mathf.Min(1f / Mathf.Max(Vector3.Dot(tangent, incoming.normalized), 1f / miterLimit), miterLimit);
                }

                for (var vertex = 0; vertex < _ringSides; vertex++)
                {
                    var angle = vertex * Mathf.PI * 2f / _ringSides;
                    var normal = side * Mathf.Cos(angle) + up * Mathf.Sin(angle);
                    var offset = normal * radius;
                    if (hasBend)
                        offset += bendAxis * (Vector3.Dot(offset, bendAxis) * (miterScale - 1f));

                    var index = ring * _ringSides + vertex;
                    _normals[index] = normal;
                    _vertices[index] = _centers[ring] + offset;
                }
            }
        }

        private static int[] BuildTriangles(int ringCount, int ringSides)
        {
            var triangles = new int[(ringCount - 1) * ringSides * 6];
            var cursor = 0;
            for (var ring = 0; ring < ringCount - 1; ring++)
            {
                for (var vertex = 0; vertex < ringSides; vertex++)
                {
                    var current = ring * ringSides + vertex;
                    var wrapped = ring * ringSides + (vertex + 1) % ringSides;
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

        private static Vector3 CatmullRom(Vector3 before, Vector3 start, Vector3 end, Vector3 after, float fraction)
        {
            var startKnot = KnotInterval(before, start);
            var endKnot = startKnot + KnotInterval(start, end);
            var afterKnot = endKnot + KnotInterval(end, after);
            var knot = Mathf.Lerp(startKnot, endKnot, fraction);

            var beforeToEnd = Vector3.LerpUnclamped(before, start, (knot - 0f) / startKnot);
            var startToEnd = Vector3.LerpUnclamped(start, end, (knot - startKnot) / (endKnot - startKnot));
            var endToAfter = Vector3.LerpUnclamped(end, after, (knot - endKnot) / (afterKnot - endKnot));
            var firstBlend = Vector3.LerpUnclamped(beforeToEnd, startToEnd, (knot - 0f) / (endKnot - 0f));
            var secondBlend = Vector3.LerpUnclamped(startToEnd, endToAfter, (knot - startKnot) / (afterKnot - startKnot));
            return Vector3.LerpUnclamped(firstBlend, secondBlend, (knot - startKnot) / (endKnot - startKnot));
        }

        private static float KnotInterval(Vector3 from, Vector3 to)
        {
            return Mathf.Sqrt(Mathf.Max((to - from).magnitude, MinFrameLength));
        }
    }
}
