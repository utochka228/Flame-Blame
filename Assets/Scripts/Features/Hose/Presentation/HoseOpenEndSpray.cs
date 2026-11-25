using FlameBlame.Hose.Components;
using FlameBlame.Water.Components;
using UnityEngine;

namespace FlameBlame.Hose.Presentation
{
    public class HoseOpenEndSpray : MonoBehaviour
    {
        private const float MinSprayPressureFraction = 0.02f;

        [SerializeField] private HoseSegment _hose;
        [SerializeField] private ParticleSystem _particles;
        [SerializeField] private PuddleField _puddles;
        [SerializeField] [Min(0f)] private float _particlesPerSecondAtFullPressure = 150f;
        [SerializeField] [Min(0f)] private float _speedAtFullPressure = 6f;
        [SerializeField] [Range(0f, 45f)] private float _spreadDegrees = 12f;

        private float[] _carries;

        private void Awake()
        {
            _carries = new float[2];
        }

        private void LateUpdate()
        {
            for (var index = 0; index < _hose.OpenEnds.Count; index++)
                Spray(_hose.OpenEnds[index], ref _carries[index]);
        }

        private void Spray(OpenEnd end, ref float carry)
        {
            var pressureFraction = _hose.PressureFractions[end.ParticleIndex];
            if (!end.IsOpen || pressureFraction < MinSprayPressureFraction)
            {
                carry = 0f;
                return;
            }

            var positions = _hose.Positions;
            var origin = positions[end.ParticleIndex];
            var outward = (origin - positions[end.InnerIndex]).normalized;
            carry += _particlesPerSecondAtFullPressure * pressureFraction * Time.deltaTime;
            var count = Mathf.FloorToInt(carry);
            carry -= count;
            for (var index = 0; index < count; index++)
                EmitOne(origin, outward, pressureFraction);

            if (_puddles)
                _puddles.AddWater(origin, _hose.Config.Pressure.LeakPuddleVolumePerSecond * pressureFraction * Time.deltaTime);
        }

        private void EmitOne(Vector3 origin, Vector3 outward, float pressureFraction)
        {
            var direction = Quaternion.AngleAxis(Random.Range(0f, _spreadDegrees), Random.onUnitSphere) * outward;
            var velocity = direction * (_speedAtFullPressure * pressureFraction);
            var isLocal = _particles.main.simulationSpace == ParticleSystemSimulationSpace.Local;
            _particles.Emit(new ParticleSystem.EmitParams
            {
                position = isLocal ? _particles.transform.InverseTransformPoint(origin) : origin,
                velocity = isLocal ? _particles.transform.InverseTransformDirection(velocity) : velocity
            }, 1);
        }
    }
}
