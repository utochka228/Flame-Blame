using FlameBlame.Audio.Components;
using FlameBlame.Audio.Config;
using FlameBlame.Core.Events;
using FlameBlame.Hose.Events;
using UnityEngine;

namespace FlameBlame.Hose.Presentation
{
    public class HoseAudio : MonoBehaviour
    {
        [SerializeField] private AudioManager _audio;
        [SerializeField] private SoundCueSO _burst;

        private void OnEnable()
        {
            EventBus.Subscribe<HoseBurst>(OnHoseBurst);
        }

        private void OnDisable()
        {
            EventBus.Unsubscribe<HoseBurst>(OnHoseBurst);
        }

        private void OnHoseBurst(HoseBurst burst)
        {
            _audio.Play(_burst);
        }
    }
}
