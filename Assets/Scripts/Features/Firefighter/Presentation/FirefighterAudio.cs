using FlameBlame.Audio.Components;
using FlameBlame.Audio.Config;
using FlameBlame.Core.Events;
using FlameBlame.Firefighter.Events;
using UnityEngine;

namespace FlameBlame.Firefighter.Presentation
{
    public class FirefighterAudio : MonoBehaviour
    {
        [SerializeField] private AudioManager _audio;
        [SerializeField] private SoundCueSO _socketLatched;
        [SerializeField] private SoundCueSO _socketReleased;
        [SerializeField] private SoundCueSO _interactionUsed;

        private void OnEnable()
        {
            EventBus.Subscribe<HoseSocketLatched>(OnSocketLatched);
            EventBus.Subscribe<HoseSocketReleased>(OnSocketReleased);
            EventBus.Subscribe<InteractionUsed>(OnInteractionUsed);
        }

        private void OnDisable()
        {
            EventBus.Unsubscribe<HoseSocketLatched>(OnSocketLatched);
            EventBus.Unsubscribe<HoseSocketReleased>(OnSocketReleased);
            EventBus.Unsubscribe<InteractionUsed>(OnInteractionUsed);
        }

        private void OnSocketLatched(HoseSocketLatched latched)
        {
            _audio.PlayAt(_socketLatched, latched.Position);
        }

        private void OnSocketReleased(HoseSocketReleased released)
        {
            _audio.PlayAt(_socketReleased, released.Position);
        }

        private void OnInteractionUsed(InteractionUsed used)
        {
            _audio.Play(_interactionUsed);
        }
    }
}
