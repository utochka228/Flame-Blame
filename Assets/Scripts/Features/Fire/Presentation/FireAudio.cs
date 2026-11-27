using FlameBlame.Audio.Components;
using FlameBlame.Audio.Config;
using FlameBlame.Core.Events;
using FlameBlame.Fire.Events;
using UnityEngine;

namespace FlameBlame.Fire.Presentation
{
    public class FireAudio : MonoBehaviour
    {
        [SerializeField] private AudioManager _audio;
        [SerializeField] private SoundCueSO _extinguished;

        private void OnEnable()
        {
            EventBus.Subscribe<FireExtinguished>(OnFireExtinguished);
        }

        private void OnDisable()
        {
            EventBus.Unsubscribe<FireExtinguished>(OnFireExtinguished);
        }

        private void OnFireExtinguished(FireExtinguished extinguished)
        {
            _audio.PlayAt(_extinguished, extinguished.Position);
        }
    }
}
