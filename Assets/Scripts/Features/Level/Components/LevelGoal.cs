using System;
using FlameBlame.Core.Events;
using FlameBlame.Fire.Events;
using FlameBlame.Firefighter.Components;
using FlameBlame.Hose.Components;
using FlameBlame.Level.Events;
using FlameBlame.Level.Simulation;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using FireComponent = FlameBlame.Fire.Components.Fire;

namespace FlameBlame.Level.Components
{
    public class LevelGoal : MonoBehaviour
    {
        [SerializeField] private FireComponent[] _fires;
        [SerializeField] private HoseNetwork _network;
        [SerializeField] private FirefighterSwap _swap;

        private LevelProgress _progress;

        public int FiresLeft => _progress.FiresLeft;
        public bool IsComplete => _progress.IsComplete;

        public event Action Changed;

        private void Awake()
        {
            _progress = new LevelProgress(_fires.Length);
        }

        private void OnEnable()
        {
            EventBus.Subscribe<FireExtinguished>(OnFireExtinguished);
            _progress.Completed += OnCompleted;
        }

        private void OnDisable()
        {
            EventBus.Unsubscribe<FireExtinguished>(OnFireExtinguished);
            _progress.Completed -= OnCompleted;
        }

        private void Update()
        {
            if (_progress.IsComplete && Keyboard.current != null && Keyboard.current.rKey.wasPressedThisFrame)
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        private void OnFireExtinguished(FireExtinguished extinguished)
        {
            var firesOut = 0;
            foreach (var fire in _fires)
            {
                if (fire.IsOut)
                    firesOut++;
            }

            _progress.Report(firesOut);
            Changed?.Invoke();
        }

        private void OnCompleted()
        {
            EventBus.Publish(new LevelCompleted(Time.timeSinceLevelLoad, _network.ConnectedHoseMetres(), _swap.SwapCount));
        }
    }
}
