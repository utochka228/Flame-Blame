using System.Collections;
using UnityEngine;

namespace FlameBlame.Fire.Presentation
{
    public class FireView : MonoBehaviour
    {
        [SerializeField] private Components.Fire _fire;
        [SerializeField] private ParticleSystem _flames;
        [SerializeField] private Light _glow;

        private Vector3 _restScale;
        private float _restGlow;
        private AudioSource[] _crackles;
        private float[] _restVolumes;
        private float _shownIntensity = 1f;
        private bool _collapsing;

        private void Awake()
        {
            _restScale = _flames.transform.localScale;
            _restGlow = _glow.intensity;
            _crackles = _flames.GetComponentsInChildren<AudioSource>();
            _restVolumes = new float[_crackles.Length];
            for (var i = 0; i < _crackles.Length; i++)
                _restVolumes[i] = _crackles[i].volume;
        }

        private void LateUpdate()
        {
            if (_collapsing) return;

            if (_fire.IsOut)
            {
                StartCoroutine(Collapse());
                return;
            }

            var config = _fire.Config;
            _shownIntensity = Mathf.Lerp(_shownIntensity, _fire.Intensity, 1f - Mathf.Exp(-config.FlameFollowSharpness * Time.deltaTime));
            ApplySize(Mathf.Lerp(config.MinFlameScaleFraction, 1f, _shownIntensity));
        }

        private IEnumerator Collapse()
        {
            _collapsing = true;
            _flames.Stop(true, ParticleSystemStopBehavior.StopEmitting);

            var from = _flames.transform.localScale.x / _restScale.x;
            var duration = _fire.Config.ExtinguishSeconds;
            for (var elapsed = 0f; elapsed < duration; elapsed += Time.deltaTime)
            {
                var t = elapsed / duration;
                ApplySize(from * (1f - t * t * t));
                yield return null;
            }

            ApplySize(0f);
            foreach (var crackle in _crackles)
                crackle.Stop();
        }

        private void ApplySize(float fraction)
        {
            _flames.transform.localScale = _restScale * fraction;
            _glow.intensity = _restGlow * fraction;
            _glow.enabled = fraction > 0f;
            for (var i = 0; i < _crackles.Length; i++)
                _crackles[i].volume = _restVolumes[i] * fraction;
        }
    }
}
