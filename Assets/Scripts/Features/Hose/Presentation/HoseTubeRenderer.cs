using FlameBlame.Hose.Components;
using UnityEngine;

namespace FlameBlame.Hose.Presentation
{
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
    public class HoseTubeRenderer : MonoBehaviour
    {
        private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");

        [SerializeField] private HoseSegment _hose;

        private Mesh _mesh;
        private MeshRenderer _meshRenderer;
        private MaterialPropertyBlock _propertyBlock;
        private TubeMeshBuilder _builder;

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
            var config = _hose.Config;
            _builder = new TubeMeshBuilder(_hose.Positions.Count, config.TubeSamplesPerSegment, config.TubeRingSides, config.TubeMiterLimit);
            _mesh.vertices = _builder.Vertices;
            _mesh.triangles = _builder.Triangles;
        }

        private void LateUpdate()
        {
            var config = _hose.Config;
            _builder.Build(_hose.Positions, config.ParticleRadius * (1f + config.SwellFraction * _hose.PressureFraction));
            _mesh.vertices = _builder.Vertices;
            _mesh.normals = _builder.Normals;
            _mesh.RecalculateBounds();

            _propertyBlock.SetColor(BaseColorId, Color.Lerp(config.RelaxedColor, config.StrainedColor, _hose.PressureFraction));
            _meshRenderer.SetPropertyBlock(_propertyBlock);
        }

        private void OnDestroy()
        {
            Destroy(_mesh);
        }
    }
}
