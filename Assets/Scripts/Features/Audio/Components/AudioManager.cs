using System.Collections.Generic;
using DG.Tweening;
using FlameBlame.Audio.Config;
using FlameBlame.Audio.Simulation;
using UnityEngine;

namespace FlameBlame.Audio.Components
{
    public class AudioManager : MonoBehaviour
    {
        private const int NoMusic = -1;

        [SerializeField] private AudioConfigSO _config;

        private readonly Dictionary<SoundCueSO, CueHistory> _history = new Dictionary<SoundCueSO, CueHistory>();
        private readonly System.Random _random = new System.Random();
        private readonly AudioSource[] _music = new AudioSource[2];
        private AudioSource[] _effects;
        private int _nextEffect;
        private int _audibleMusic = NoMusic;
        private Tween _musicFade;

        private void Awake()
        {
            _effects = new AudioSource[_config.PoolSize];
            for (var index = 0; index < _effects.Length; index++)
                _effects[index] = CreateEffectSource();

            for (var index = 0; index < _music.Length; index++)
                _music[index] = CreateMusicSource(index);
        }

        private void OnDestroy()
        {
            _musicFade.Kill();
        }

        public void Play(SoundCueSO cue)
        {
            Emit(cue, transform.position, 0f);
        }

        public void PlayAt(SoundCueSO cue, Vector3 position)
        {
            Emit(cue, position, 1f);
        }

        public void PlayMusic(AudioClip clip)
        {
            _audibleMusic = _audibleMusic == 0 ? 1 : 0;
            var incoming = _music[_audibleMusic];
            incoming.clip = clip;
            incoming.volume = 0f;
            incoming.Play();
            FadeMusic();
        }

        public void StopMusic()
        {
            _audibleMusic = NoMusic;
            FadeMusic();
        }

        public void SetVolume(AudioBus bus, float sliderFraction)
        {
            _config.Mixer.SetFloat(_config.RouteFor(bus).VolumeParameter, VolumeCurve.ToDecibels(sliderFraction));
        }

        private void Emit(SoundCueSO cue, Vector3 position, float spatialBlend)
        {
            if (!cue || cue.Takes.Count == 0)
                return;

            var history = HistoryOf(cue);
            var now = Time.unscaledTime;
            if (now < history.NextPlayTime)
                return;

            history.NextPlayTime = now + cue.MinIntervalSeconds;
            history.LastTake = TakePicker.Pick(cue.Takes.Count, history.LastTake, _random);

            var source = TakeEffectSource();
            source.transform.position = position;
            source.spatialBlend = spatialBlend;
            source.outputAudioMixerGroup = _config.RouteFor(cue.Bus).Group;
            source.clip = cue.Takes[history.LastTake];
            source.volume = cue.VolumeFraction;
            source.pitch = 1f + cue.PitchJitterFraction * (float)(_random.NextDouble() * 2.0 - 1.0);
            source.Play();
        }

        private AudioSource TakeEffectSource()
        {
            var chosen = _nextEffect;
            for (var offset = 0; offset < _effects.Length; offset++)
            {
                var index = (_nextEffect + offset) % _effects.Length;
                if (_effects[index].isPlaying)
                    continue;

                chosen = index;
                break;
            }

            _nextEffect = (chosen + 1) % _effects.Length;
            return _effects[chosen];
        }

        private CueHistory HistoryOf(SoundCueSO cue)
        {
            if (_history.TryGetValue(cue, out var history))
                return history;

            history = new CueHistory();
            _history.Add(cue, history);
            return history;
        }

        private void FadeMusic()
        {
            _musicFade.Kill();
            var sequence = DOTween.Sequence().SetUpdate(true).SetLink(gameObject);
            for (var index = 0; index < _music.Length; index++)
            {
                var source = _music[index];
                var target = index == _audibleMusic ? 1f : 0f;
                sequence.Join(DOTween.To(() => source.volume, volume => source.volume = volume, target, _config.MusicFadeSeconds));
            }

            sequence.OnComplete(StopSilentMusic);
            _musicFade = sequence;
        }

        private void StopSilentMusic()
        {
            for (var index = 0; index < _music.Length; index++)
            {
                if (index != _audibleMusic)
                    _music[index].Stop();
            }
        }

        private AudioSource CreateEffectSource()
        {
            var source = CreateSource("Sfx");
            source.rolloffMode = AudioRolloffMode.Linear;
            source.minDistance = _config.SpatialMinDistanceMetres;
            source.maxDistance = _config.SpatialMaxDistanceMetres;
            return source;
        }

        private AudioSource CreateMusicSource(int index)
        {
            var source = CreateSource($"Music{index}");
            source.outputAudioMixerGroup = _config.RouteFor(AudioBus.Music).Group;
            source.loop = true;
            source.priority = 0;
            return source;
        }

        private AudioSource CreateSource(string sourceName)
        {
            var host = new GameObject(sourceName);
            host.transform.SetParent(transform, false);
            var source = host.AddComponent<AudioSource>();
            source.playOnAwake = false;
            return source;
        }

        private sealed class CueHistory
        {
            public int LastTake = TakePicker.NoTake;
            public float NextPlayTime;
        }
    }
}
