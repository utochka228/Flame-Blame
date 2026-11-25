using System;
using NaughtyAttributes;
using UnityEngine;

namespace FlameBlame.Core.Config
{
    public abstract class ConfigSO : ScriptableObject
    {
        [SerializeField] [BoxGroup("Debug")] private bool _liveTuning = true;

        public event Action Changed;

        [Button("Apply Now", EButtonEnableMode.Playmode)]
        public void NotifyChanged()
        {
            Changed?.Invoke();
        }

        private void OnValidate()
        {
            if (_liveTuning && Application.isPlaying)
                Changed?.Invoke();
        }
    }
}
