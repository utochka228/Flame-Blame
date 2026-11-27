using FlameBlame.Core.Events;
using FlameBlame.Firefighter.Events;
using UnityEngine;

namespace FlameBlame.Firefighter.Presentation
{
    [RequireComponent(typeof(ParticleSystem))]
    public class SlipSplash : MonoBehaviour
    {
        [SerializeField] [Range(1, 40)] private int _particleCount = 10;

        private ParticleSystem _particles;

        private void Awake()
        {
            _particles = GetComponent<ParticleSystem>();
        }

        private void OnEnable()
        {
            EventBus.Subscribe<FirefighterSlipped>(OnSlipped);
        }

        private void OnDisable()
        {
            EventBus.Unsubscribe<FirefighterSlipped>(OnSlipped);
        }

        private void OnSlipped(FirefighterSlipped slipped)
        {
            transform.position = slipped.Position;
            _particles.Emit(_particleCount);
        }
    }
}
