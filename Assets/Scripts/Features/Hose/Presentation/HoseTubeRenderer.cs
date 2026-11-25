using System.Collections.Generic;
using FlameBlame.Hose.Components;
using FlameBlame.Hose.Config;
using UnityEngine;

namespace FlameBlame.Hose.Presentation
{
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
    public class HoseTubeRenderer : MonoBehaviour
    {
        private const float ShakePhaseStepX = 1.7f;
        private const float ShakePhaseStepY = 2.3f;
        private const float ShakePhaseStepZ = 3.1f;
        private const float ShakeSpeedY = 1.3f;
        private const float ShakeSpeedZ = 0.9f;

        private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
        private static readonly int BaseMapId = Shader.PropertyToID("_BaseMap");

        [SerializeField] private HoseSegment _hose;
        [SerializeField] private HosePinchSensor _pinchSensor;
        [SerializeField] private HoseBurstMonitor _burstMonitor;

        private Mesh _mesh;
        private MeshRenderer _meshRenderer;
        private MaterialPropertyBlock _propertyBlock;
        private TubeMeshBuilder _builder;
        private HoseRadiusProfile _radiusProfile;
        private HoseTubePaint _paint;
        private float[] _radii;
        private Vector3[] _shakenPositions;

        private void Awake()
        {
            _meshRenderer = GetComponent<MeshRenderer>();
            _propertyBlock = new MaterialPropertyBlock();
            _mesh = new Mesh { name = "HoseTube" };
            _mesh.MarkDynamic();
            GetComponent<MeshFilter>().sharedMesh = _mesh;
            transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
        }

        private void OnEnable()
        {
            _hose.Config.Changed += OnConfigChanged;
        }

        private void OnDisable()
        {
            _hose.Config.Changed -= OnConfigChanged;
        }

        private void Start()
        {
            var config = _hose.Config;
            _builder = new TubeMeshBuilder(_hose.Positions.Count, config.Tube.SamplesPerSegment, config.Tube.RingSides, config.Tube.MiterLimit);
            _radiusProfile = new HoseRadiusProfile(config.Rope.ParticleRadius, config.Tube.SwellFraction, config.Pinch.SquashFraction);
            _radii = new float[_hose.Positions.Count];
            _shakenPositions = new Vector3[_hose.Positions.Count];
            _mesh.vertices = _builder.Vertices;
            _mesh.uv = _builder.Uvs;
            _mesh.triangles = _builder.Triangles;
            _paint = new HoseTubePaint(config);
        }

        private void OnConfigChanged()
        {
            var config = _hose.Config;
            _radiusProfile = new HoseRadiusProfile(config.Rope.ParticleRadius, config.Tube.SwellFraction, config.Pinch.SquashFraction);
            _paint.ApplyColors(config);
        }

        private void LateUpdate()
        {
            var config = _hose.Config;
            var burstProgress = _burstMonitor.ProgressFraction;

            _radiusProfile.Fill(_hose.PressureFractions, _pinchSensor.Pinched, _radii);
            IReadOnlyList<Vector3> points = _hose.Positions;
            if (burstProgress > 0f)
            {
                ApplyBulge(config, burstProgress);
                ApplyShake(config, burstProgress);
                points = _shakenPositions;
            }

            _builder.Build(points, _radii);
            _mesh.vertices = _builder.Vertices;
            _mesh.normals = _builder.Normals;
            _mesh.RecalculateBounds();

            var tint = Color.Lerp(config.Colors.RelaxedColor, config.Colors.StrainedColor, _hose.PressureFractions[_burstMonitor.WeakPointIndex]);
            _paint.Update(_burstMonitor.Breaches, tint);
            _propertyBlock.SetColor(BaseColorId, Color.white);
            _propertyBlock.SetTexture(BaseMapId, _paint.Texture);
            _meshRenderer.SetPropertyBlock(_propertyBlock);
        }

        private void OnDestroy()
        {
            Destroy(_mesh);
            _paint.Destroy();
        }

        private void ApplyBulge(HoseConfigSO config, float burstProgress)
        {
            _radiusProfile.AddBulge(_radii, _burstMonitor.WeakPointIndex, config.Burst.BalloonWidthParticles, config.Burst.BalloonFraction * burstProgress);
        }

        private void ApplyShake(HoseConfigSO config, float burstProgress)
        {
            var center = _burstMonitor.WeakPointIndex;
            var phase = Time.time * config.Burst.ShakeHertz * Mathf.PI * 2f;
            for (var index = 0; index < _shakenPositions.Length; index++)
            {
                var weight = HoseRadiusProfile.FalloffWeight(index, center, config.Burst.BalloonWidthParticles);
                var jitter = new Vector3(
                    Mathf.Sin(phase + index * ShakePhaseStepX),
                    Mathf.Sin(phase * ShakeSpeedY + index * ShakePhaseStepY),
                    Mathf.Sin(phase * ShakeSpeedZ + index * ShakePhaseStepZ));
                _shakenPositions[index] = _hose.Positions[index] + jitter * (config.Burst.ShakeAmplitude * burstProgress * weight);
            }
        }
    }
}
