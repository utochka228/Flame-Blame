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

        public float WarningFraction => _timer.WarningFraction;
        public float VisorFraction => _transition.VisorFraction;
        public bool IsFirstActive => _firstIsActive;

        public event System.Action Swapped;

        private void Awake()
        {
            Cursor.lockState = CursorLockMode.Locked;
            _timer = new SwapTimer(_config.IntervalSeconds, _config.WarningSeconds);
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
            if (Keyboard.current != null && Keyboard.current.tabKey.wasPressedThisFrame && !_transition.IsActive)
                BeginSwap();

            if (_timer.Step(Time.deltaTime, CanForceSwap()))
                BeginSwap();

            if (_transition.Step(Time.deltaTime))
                CutToOther();

            var commands = _input.Read();
            _first.Drive(_firstIsActive ? commands : default);
            _second.Drive(_firstIsActive ? default : commands);
        }

        public void StartForcedTimer()
        {
            if (_isTimerStarted) return;

            _isTimerStarted = true;
            RestartTimer();
        }

        private void BeginSwap()
        {
            _timer.Stop();
            _transition.Begin();
        }

        private void CutToOther()
        {
            _firstIsActive = !_firstIsActive;
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
            _timer.Configure(_config.IntervalSeconds, _config.WarningSeconds);
            _transition.Configure(_config.VisorDropSeconds, _config.VisorLiftSeconds);
            if (_isTimerStarted && !_transition.IsActive)
                RestartTimer();
        }
    }
}
