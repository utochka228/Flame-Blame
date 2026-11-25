using System.Collections.Generic;
using UnityEngine;

namespace FlameBlame.Hose.Components
{
    public class HosePinchSensor : MonoBehaviour
    {
        private const int MaxFootOverlaps = 4;

        [SerializeField] private HoseSegment _hose;

        private readonly Collider[] _feet = new Collider[MaxFootOverlaps];
        private bool[] _pinched;
        private float _secondsSinceCheck;

        public IReadOnlyList<bool> Pinched => _pinched;

        private void Awake()
        {
            _pinched = new bool[_hose.Config.ParticleCount];
        }

        private void FixedUpdate()
        {
            _secondsSinceCheck += Time.fixedDeltaTime;
            if (_secondsSinceCheck < 1f / _hose.Config.Pinch.ChecksPerSecond) return;

            _secondsSinceCheck = 0f;
            Check();
        }

        private void Check()
        {
            var positions = _hose.Positions;
            var config = _hose.Config;
            for (var index = 0; index < _pinched.Length; index++)
            {
                _pinched[index] = Physics.CheckSphere(positions[index], config.Rope.ParticleRadius, config.Pinch.Layers,
                                      QueryTriggerInteraction.Ignore) || IsUnderFoot(positions[index]);
            }
        }

        private bool IsUnderFoot(Vector3 position)
        {
            var config = _hose.Config;
            var count = Physics.OverlapSphereNonAlloc(position, config.Rope.ParticleRadius, _feet, config.Pinch.FootLayers,
                QueryTriggerInteraction.Ignore);
            for (var index = 0; index < count; index++)
            {
                if (position.y <= _feet[index].bounds.min.y + config.Pinch.FootHeightMeters)
                    return true;
            }

            return false;
        }
    }
}
