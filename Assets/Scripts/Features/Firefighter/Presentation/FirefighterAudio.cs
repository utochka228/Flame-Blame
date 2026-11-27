using FlameBlame.Audio.Components;
using FlameBlame.Audio.Config;
using FlameBlame.Core.Events;
using FlameBlame.Firefighter.Events;
using FlameBlame.Firefighter.Simulation;
using UnityEngine;

namespace FlameBlame.Firefighter.Presentation
{
    public class FirefighterAudio : MonoBehaviour
    {
        [SerializeField] private AudioManager _audio;
        [SerializeField] private SoundCueSO _socketLatched;
        [SerializeField] private SoundCueSO _socketReleased;
        [SerializeField] private SoundCueSO _interactionUsed;
        [SerializeField] private SoundCueSO _slipped;
        [SerializeField] private SoundCueSO _grabbed;
        [SerializeField] private SoundCueSO _thrown;
        [SerializeField] private SoundCueSO _taped;
        [SerializeField] private SoundCueSO _valveStep;
        [SerializeField] private SoundCueSO _valveLimit;
        [SerializeField] private Vector2 _valveStepPitchRange = new Vector2(0.8f, 1.4f);

        private void OnEnable()
        {
            EventBus.Subscribe<HoseSocketLatched>(OnSocketLatched);
            EventBus.Subscribe<HoseSocketReleased>(OnSocketReleased);
            EventBus.Subscribe<InteractionUsed>(OnInteractionUsed);
            EventBus.Subscribe<FirefighterSlipped>(OnSlipped);
            EventBus.Subscribe<ValveAdjusted>(OnValveAdjusted);
            EventBus.Subscribe<HoseGrabbed>(OnGrabbed);
            EventBus.Subscribe<HoseThrown>(OnThrown);
        }

        private void OnDisable()
        {
            EventBus.Unsubscribe<HoseSocketLatched>(OnSocketLatched);
            EventBus.Unsubscribe<HoseSocketReleased>(OnSocketReleased);
            EventBus.Unsubscribe<InteractionUsed>(OnInteractionUsed);
            EventBus.Unsubscribe<FirefighterSlipped>(OnSlipped);
            EventBus.Unsubscribe<ValveAdjusted>(OnValveAdjusted);
            EventBus.Unsubscribe<HoseGrabbed>(OnGrabbed);
            EventBus.Unsubscribe<HoseThrown>(OnThrown);
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
            _audio.Play(used.Action == UseAction.Tape ? _taped : _interactionUsed);
        }

        private void OnGrabbed(HoseGrabbed grabbed)
        {
            _audio.PlayAt(_grabbed, grabbed.Position);
        }

        private void OnThrown(HoseThrown thrown)
        {
            _audio.PlayAt(_thrown, thrown.Position);
        }

        private void OnSlipped(FirefighterSlipped slipped)
        {
            _audio.PlayAt(_slipped, slipped.Position);
        }

        private void OnValveAdjusted(ValveAdjusted adjusted)
        {
            var position = adjusted.Valve.transform.position;
            if (adjusted.IsAtLimit)
            {
                _audio.PlayAt(_valveLimit, position);
                return;
            }

            var pitch = Mathf.Lerp(_valveStepPitchRange.x, _valveStepPitchRange.y, adjusted.Valve.PressureFraction);
            _audio.PlayAt(_valveStep, position, pitch);
        }
    }
}
