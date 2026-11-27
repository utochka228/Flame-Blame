using System;
using UnityEngine;
using UnityEngine.Audio;

namespace FlameBlame.Audio.Config
{
    [Serializable]
    public class BusRoute
    {
        [field: SerializeField] public AudioBus Bus { get; private set; }
        [field: SerializeField] public AudioMixerGroup Group { get; private set; }
        [field: SerializeField] public string VolumeParameter { get; private set; }
    }
}
