using System.Collections.Generic;
using FlameBlame.Core.Config;
using UnityEngine;
using UnityEngine.Audio;

namespace FlameBlame.Audio.Config
{
    [CreateAssetMenu(fileName = "AudioConfig", menuName = "FlameBlame/Configs/Audio")]
    public class AudioConfigSO : ConfigSO
    {
        [SerializeField] private List<BusRoute> _routes = new List<BusRoute>();

        [field: Header("Mixer")]
        [field: SerializeField] public AudioMixer Mixer { get; private set; }

        [field: Header("Pool")]
        [field: SerializeField] [field: Min(1)] public int PoolSize { get; private set; } = 16;

        [field: Header("Spatial")]
        [field: SerializeField] [field: Min(0f)] public float SpatialMinDistanceMetres { get; private set; } = 2f;
        [field: SerializeField] [field: Min(0f)] public float SpatialMaxDistanceMetres { get; private set; } = 30f;

        [field: Header("Music")]
        [field: SerializeField] [field: Min(0f)] public float MusicFadeSeconds { get; private set; } = 1.5f;

        public BusRoute RouteFor(AudioBus bus)
        {
            foreach (var route in _routes)
            {
                if (route.Bus == bus)
                    return route;
            }

            throw new KeyNotFoundException($"No mixer route for {bus}");
        }
    }
}
