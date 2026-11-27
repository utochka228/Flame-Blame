using FlameBlame.Audio.Components;
using FlameBlame.Audio.Config;
using FlameBlame.Firefighter.Components;
using FlameBlame.Firefighter.Config;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;
using Cursor = UnityEngine.Cursor;

namespace FlameBlame.UI.Menu
{
    [RequireComponent(typeof(UIDocument))]
    public class MenuPresenter : MonoBehaviour
    {
        private static bool _hasStartedThisSession;

        [SerializeField] private FirefighterSwap _swap;
        [SerializeField] private FirefighterSwapConfigSO _swapConfig;
        [SerializeField] private AudioManager _audio;
        [SerializeField] private SoundCueSO _click;
        [SerializeField] private SoundCueSO _back;

        private MenuScreen _menu;
        private BriefingWindow _briefing;
        private MenuSettings _settings;
        private MenuState _state;
        private VisualElement _root;

        private void OnEnable()
        {
            _root = GetComponent<UIDocument>().rootVisualElement;
            _menu = new MenuScreen(_root);
            _briefing = new BriefingWindow(_root);
            _root.Q<Label>("companyLabel").text = $"© {Application.companyName} · not liable for wet carpets";

            _menu.PlayClicked += OnPlayClicked;
            _menu.ModeSelected += OnModeSelected;
            _menu.SfxVolumeChanged += OnSfxVolumeChanged;
            _menu.MusicVolumeChanged += OnMusicVolumeChanged;
            _briefing.Finished += OnBriefingFinished;
            _root.RegisterCallback<ClickEvent>(OnClicked);
        }

        private void Start()
        {
            _settings = MenuSettings.Load(_swapConfig.Interval);
            _swap.SetInterval(_settings.Swap);
            OnSfxVolumeChanged(_settings.SfxVolume);
            OnMusicVolumeChanged(_settings.MusicVolume);

            if (_hasStartedThisSession)
                Resume();
            else
                OpenMenu(MenuState.Start);
        }

        private void OnDisable()
        {
            _menu.PlayClicked -= OnPlayClicked;
            _menu.ModeSelected -= OnModeSelected;
            _menu.SfxVolumeChanged -= OnSfxVolumeChanged;
            _menu.MusicVolumeChanged -= OnMusicVolumeChanged;
            _briefing.Finished -= OnBriefingFinished;
            _root.UnregisterCallback<ClickEvent>(OnClicked);
            _menu.Dispose();
            _briefing.Dispose();
        }

        private void Update()
        {
            var keyboard = Keyboard.current;
            if (keyboard == null) return;

            if (keyboard.escapeKey.wasPressedThisFrame && _state != MenuState.Start && _state != MenuState.Briefing)
            {
                _audio.Play(_back);
                if (_state == MenuState.Playing)
                    OpenMenu(MenuState.Paused);
                else if (_state == MenuState.Paused)
                    OnPlayClicked();
            }

            if (_state == MenuState.Briefing && (keyboard.enterKey.wasPressedThisFrame || keyboard.spaceKey.wasPressedThisFrame))
                _briefing.Next();
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetSession()
        {
            _hasStartedThisSession = false;
        }

        private void OnClicked(ClickEvent click)
        {
            if (click.target is Button)
                _audio.Play(_click);
        }

        private void OpenMenu(MenuState state)
        {
            _state = state;
            Freeze();
            _menu.Show(state == MenuState.Paused, _settings);
        }

        private void OnPlayClicked()
        {
            _settings.Save();
            _menu.Hide();
            if (_state != MenuState.Start)
            {
                Resume();
                return;
            }

            _hasStartedThisSession = true;
            _state = MenuState.Briefing;
            _briefing.Show();
        }

        private void OnBriefingFinished()
        {
            Resume();
        }

        private void Freeze()
        {
            Time.timeScale = 0f;
            _swap.IsInputEnabled = false;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        private void Resume()
        {
            _state = MenuState.Playing;
            Time.timeScale = 1f;
            _swap.IsInputEnabled = true;
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        private void OnModeSelected(SwapInterval mode)
        {
            _settings.Swap = mode;
            _swap.SetInterval(mode);
        }

        private void OnSfxVolumeChanged(float fraction)
        {
            _settings.SfxVolume = fraction;
            _audio.SetVolume(AudioBus.Sfx, fraction);
            _audio.SetVolume(AudioBus.Ui, fraction);
            _audio.SetVolume(AudioBus.Ambience, fraction);
        }

        private void OnMusicVolumeChanged(float fraction)
        {
            _settings.MusicVolume = fraction;
            _audio.SetVolume(AudioBus.Music, fraction);
        }
    }
}
