using FlameBlame.Audio.Components;
using FlameBlame.Audio.Config;
using FlameBlame.Firefighter.Components;
using FlameBlame.Firefighter.Config;
using FlameBlame.Water.Components;
using UnityEngine;

namespace FlameBlame.Firefighter.Presentation
{
    public class FirefighterFootsteps : MonoBehaviour
    {
        [SerializeField] private FirefighterBody _body;
        [SerializeField] private FirefighterConfigSO _config;
        [SerializeField] private AudioManager _audio;
        [SerializeField] private PuddleField _puddles;

        [Header("Cues")]
        [SerializeField] private SoundCueSO _defaultStep;
        [SerializeField] private SoundCueSO _grassStep;
        [SerializeField] private SoundCueSO _mudStep;
        [SerializeField] private SoundCueSO _waterStep;

        [Header("Cadence")]
        [SerializeField] [Min(0.1f)] private float _strideMetres = 1.6f;
        [SerializeField] [Min(0.1f)] private float _surfaceProbeMetres = 1.5f;

        private float _stridedMetres;

        private void Update()
        {
            if (!_body.IsGrounded || _body.IsFallen)
                return;

            var velocity = _body.Velocity;
            _stridedMetres += new Vector2(velocity.x, velocity.z).magnitude * Time.deltaTime;
            if (_stridedMetres < _strideMetres)
                return;

            _stridedMetres = 0f;
            var position = _body.transform.position;
            _audio.PlayAt(CueFor(SurfaceAt(position)), position);
        }

        private FootstepSurfaceKind SurfaceAt(Vector3 position)
        {
            if (_puddles && _puddles.TractionAt(position) < 1f)
                return FootstepSurfaceKind.Water;

            if (!Physics.Raycast(position, Vector3.down, out var hit, _surfaceProbeMetres, _config.Movement.GroundLayers, QueryTriggerInteraction.Ignore))
                return FootstepSurfaceKind.Default;

            var surface = hit.collider.GetComponentInParent<FootstepSurface>();
            return surface ? surface.Kind : FootstepSurfaceKind.Default;
        }

        private SoundCueSO CueFor(FootstepSurfaceKind kind)
        {
            switch (kind)
            {
                case FootstepSurfaceKind.Grass:
                    return _grassStep;
                case FootstepSurfaceKind.Mud:
                    return _mudStep;
                case FootstepSurfaceKind.Water:
                    return _waterStep;
                default:
                    return _defaultStep;
            }
        }
    }
}
