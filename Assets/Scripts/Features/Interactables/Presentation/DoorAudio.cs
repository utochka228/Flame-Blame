using FlameBlame.Audio.Components;
using FlameBlame.Audio.Config;
using FlameBlame.Core.Events;
using FlameBlame.Interactables.Events;
using UnityEngine;

namespace FlameBlame.Interactables.Presentation
{
    public class DoorAudio : MonoBehaviour
    {
        [SerializeField] private AudioManager _audio;
        [SerializeField] private SoundCueSO _opened;

        private void OnEnable()
        {
            EventBus.Subscribe<DoorOpened>(OnDoorOpened);
        }

        private void OnDisable()
        {
            EventBus.Unsubscribe<DoorOpened>(OnDoorOpened);
        }

        private void OnDoorOpened(DoorOpened opened)
        {
            _audio.PlayAt(_opened, opened.Position);
        }
    }
}
