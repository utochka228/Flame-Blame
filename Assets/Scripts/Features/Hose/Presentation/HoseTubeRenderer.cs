using System.Collections.Generic;
using FlameBlame.Hose.Components;
using FlameBlame.Hose.Config;
using UnityEngine;

namespace FlameBlame.Hose.Presentation
{
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
    public class HoseTubeRenderer : MonoBehaviour
    {
        private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");

        [SerializeField] private HoseSegment _hose;
        [SerializeField] private HosePinchSensor _pinchSensor;
        [SerializeField] private HoseBurstMonitor _burstMonitor;

        private Mesh _mesh;
        private MeshRenderer _meshRenderer;
        private MaterialPropertyBlock _propertyBlock;
        private TubeMeshBuilder _builder;
        private HoseRadiusProfile _radiusProfile;
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

        private void Start()
        {
            var config = _hose.Config;
            _builder = new TubeMeshBuilder(_hose.Positions.Count, config.TubeSamplesPerSegment, config.TubeRingSides, config.TubeMiterLimit);
            _radiusProfile = new HoseRadiusProfile(config.ParticleRadius, config.SwellFraction, config.PinchSquashFraction);
            _radii = new float[_hose.Positions.Count];
            _shakenPositions = new Vector3[_hose.Positions.Count];
            _mesh.vertices = _builder.Vertices;
            _mesh.triangles = _builder.Triangles;
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

            _propertyBlock.SetColor(BaseColorId, Color.Lerp(config.RelaxedColor, config.StrainedColor, _hose.InletPressureFraction));
            _meshRenderer.SetPropertyBlock(_propertyBlock);
        }

        private void OnDestroy()
        {
            Destroy(_mesh);
        }

        private void ApplyBulge(HoseConfigSO config, float burstProgress)
        {
            _radiusProfile.AddBulge(_radii, _burstMonitor.WeakPointIndex, config.BalloonWidthParticles, config.BalloonFraction * burstProgress);
        }

        private void ApplyShake(HoseConfigSO config, float burstProgress)
        {
            var center = _burstMonitor.WeakPointIndex;
            var phase = Time.time * config.ShakeHertz * Mathf.PI * 2f;
            for (var index = 0; index < _shakenPositions.Length; index++)
            {
                var weight = HoseRadiusProfile.FalloffWeight(index, center, config.BalloonWidthParticles);
                var jitter = new Vector3(Mathf.Sin(phase + index * 1.7f), Mathf.Sin(phase * 1.3f + index * 2.3f), Mathf.Sin(phase * 0.9f + index * 3.1f));
                _shakenPositions[index] = _hose.Positions[index] + jitter * (config.ShakeAmplitude * burstProgress * weight);
            }
        }
    }
}
