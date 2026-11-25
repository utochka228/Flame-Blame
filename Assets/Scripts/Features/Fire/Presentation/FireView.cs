using FlameBlame.Fire.Components;
using UnityEngine;

namespace FlameBlame.Fire.Presentation
{
    public class FireView : MonoBehaviour
    {
        [SerializeField] private Components.Fire _fire;
        [SerializeField] private ParticleSystem _flames;

        private void LateUpdate()
        {
            var emission = _flames.emission;
            emission.rateOverTime = _fire.Config.FlameParticlesPerSecond * _fire.Intensity;
            if (_fire.IsOut && _flames.isEmitting)
                _flames.Stop(true, ParticleSystemStopBehavior.StopEmitting);
        }
    }
}
