using FlameBlame.Audio.Components;
using FlameBlame.Audio.Config;
using FlameBlame.Core.Events;
using FlameBlame.Level.Events;
using UnityEngine;

namespace FlameBlame.Level.Presentation
{
    public class LevelAudio : MonoBehaviour
    {
        [SerializeField] private AudioManager _audio;
        [SerializeField] private AudioClip _music;
        [SerializeField] private SoundCueSO _completed;

        private void OnEnable()
        {
            EventBus.Subscribe<LevelCompleted>(OnLevelCompleted);
        }

        private void Start()
        {
            _audio.PlayMusic(_music);
        }

        private void OnDisable()
        {
            EventBus.Unsubscribe<LevelCompleted>(OnLevelCompleted);
        }

        private void OnLevelCompleted(LevelCompleted completed)
        {
            _audio.StopMusic();
            _audio.Play(_completed);
        }
    }
}
