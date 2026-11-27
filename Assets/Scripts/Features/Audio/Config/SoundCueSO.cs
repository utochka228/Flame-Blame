using System.Collections.Generic;
using FlameBlame.Core.Extensions;
using UnityEngine;

namespace FlameBlame.Audio.Config
{
    [CreateAssetMenu(fileName = "SoundCue", menuName = "FlameBlame/Audio/Sound Cue")]
    public class SoundCueSO : ScriptableObject
    {
        [SerializeField] private AudioClip[] _takes = System.Array.Empty<AudioClip>();
        [SerializeField] [Range(0, 100)] private int _volumePercent = 100;
        [SerializeField] [Range(0, 50)] private int _pitchJitterPercent = 5;

        [field: SerializeField] public AudioBus Bus { get; private set; }
        [field: SerializeField] [field: Min(0f)] public float MinIntervalSeconds { get; private set; } = 0.05f;

        public IReadOnlyList<AudioClip> Takes => _takes;
        public float VolumeFraction => _volumePercent.ToFraction();
        public float PitchJitterFraction => _pitchJitterPercent.ToFraction();
    }
}
