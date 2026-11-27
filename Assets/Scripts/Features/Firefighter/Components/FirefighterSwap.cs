using FlameBlame.Firefighter.Config;
using FlameBlame.Firefighter.Simulation;
using UnityEngine;
using UnityEngine.InputSystem;

namespace FlameBlame.Firefighter.Components
{
    public class FirefighterSwap : MonoBehaviour
    {
        [SerializeField] private FirefighterSwapConfigSO _config;
        [SerializeField] private FirefighterBody _first;
        [SerializeField] private FirefighterBody _second;
        [SerializeField] private HoseSockets _sockets;

        private readonly KeyboardMouseInput _input = new();

        private SwapTimer _timer;
        private SwapTransition _transition;
        private HoseClimber _firstClimber;
        private HoseClimber _secondClimber;
        private FirefighterInteractor _firstInteractor;
        private FirefighterInteractor _secondInteractor;
        private bool _firstIsActive = true;
        private bool _isTimerStarted;
        private float _intervalSeconds;

        public float WarningFraction => _timer.WarningFraction;
        public float SecondsLeft => _timer.SecondsLeft;
        public float VisorFraction => _transition.VisorFraction;
        public bool IsFirstActive => _firstIsActive;
        public FirefighterCommands Commands { get; private set; }
        public int SwapCount { get; private set; }
        public bool IsInputEnabled { get; set; } = true;

        public event System.Action Swapped;

        private void Awake()
        {
            Cursor.lockState = CursorLockMode.Locked;
            _intervalSeconds = _config.IntervalSeconds;
            _timer = new SwapTimer(_intervalSeconds, _config.WarningSeconds);
            _transition = new SwapTransition(_config.VisorDropSeconds, _config.VisorLiftSeconds);
            _firstClimber = _first.GetComponent<HoseClimber>();
            _secondClimber = _second.GetComponent<HoseClimber>();
            _firstInteractor = _first.GetComponent<FirefighterInteractor>();
            _secondInteractor = _second.GetComponent<FirefighterInteractor>();
            ApplyActive();
        }

        private void OnEnable()
        {
            _config.Changed += OnConfigChanged;
        }

        private void OnDisable()
        {
            _config.Changed -= OnConfigChanged;
        }

        private void Update()
        {
            if (IsInputEnabled && Keyboard.current != null && Keyboard.current.tabKey.wasPressedThisFrame && !_transition.IsActive)
                BeginSwap();

            if (_timer.Step(Time.deltaTime, CanForceSwap()))
                BeginSwap();

            if (_transition.Step(Time.deltaTime))
                CutToOther();

            Commands = IsInputEnabled ? _input.Read() : default;
            _first.Drive(_firstIsActive ? Commands : default);
            _second.Drive(_firstIsActive ? default : Commands);
        }

        public void StartForcedTimer()
        {
            if (_isTimerStarted) return;

            _isTimerStarted = true;
            RestartTimer();
        }

        public void SetInterval(SwapInterval interval)
        {
            _intervalSeconds = (int)interval;
            ApplyTimerSettings();
        }

        private void BeginSwap()
        {
            _timer.Stop();
            _transition.Begin();
        }

        private void CutToOther()
        {
            _firstIsActive = !_firstIsActive;
            SwapCount++;
            ApplyActive();
            Swapped?.Invoke();
            if (_isTimerStarted)
                RestartTimer();
        }

        private void RestartTimer()
        {
            _timer.Restart(Random.value);
        }

        private bool CanForceSwap()
        {
            var climber = _firstIsActive ? _firstClimber : _secondClimber;
            return !climber.IsMantling && !_sockets.IsCatchArmed;
        }

        private void ApplyActive()
        {
            _first.Eye.gameObject.SetActive(_firstIsActive);
            _second.Eye.gameObject.SetActive(!_firstIsActive);
            _firstInteractor.enabled = _firstIsActive;
            _secondInteractor.enabled = !_firstIsActive;
        }

        private void OnConfigChanged()
        {
            _intervalSeconds = _config.IntervalSeconds;
            _transition.Configure(_config.VisorDropSeconds, _config.VisorLiftSeconds);
            ApplyTimerSettings();
        }

        private void ApplyTimerSettings()
        {
            _timer.Configure(_intervalSeconds, _config.WarningSeconds);
            if (_isTimerStarted && !_transition.IsActive)
                RestartTimer();
        }
    }
}
