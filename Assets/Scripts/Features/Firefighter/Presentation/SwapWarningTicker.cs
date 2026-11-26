using FlameBlame.Firefighter.Components;
using FlameBlame.Firefighter.Config;
using UnityEngine;

namespace FlameBlame.Firefighter.Presentation
{
    [RequireComponent(typeof(AudioSource))]
    public class SwapWarningTicker : MonoBehaviour
    {
        [SerializeField] private FirefighterSwapConfigSO _config;
        [SerializeField] private FirefighterSwap _swap;

        private AudioSource _source;
        private float _secondsToTick;

        private void Awake()
        {
            _source = GetComponent<AudioSource>();
        }

        private void Update()
        {
            var warning = _swap.WarningFraction;
            if (warning <= 0f)
            {
                _secondsToTick = 0f;
                return;
            }

            _secondsToTick -= Time.deltaTime;
            if (_secondsToTick > 0f) return;

            _source.Play();
            _secondsToTick = Mathf.Lerp(_config.SlowestTickSeconds, _config.FastestTickSeconds, warning);
        }
    }
}
