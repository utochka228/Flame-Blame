using System;
using DG.Tweening;
using FlameBlame.Core.Extensions;
using FlameBlame.Firefighter.Config;
using FlameBlame.UI.Kit;
using UnityEngine;
using UnityEngine.UIElements;

namespace FlameBlame.UI.Menu
{
    public class MenuScreen : IDisposable
    {
        private const string HiddenClass = "menu--hidden";
        private const string SelectedClass = "fb-button--selected";
        private const string PlayText = "PLAY (responsibly)";
        private const string ResumeText = "BACK TO THE FIRE";
        private const string StartTagline = "Put out fires. Blame coworkers. Repeat.";
        private const string PauseTagline = "Paused. The fire is waiting politely.";
        private const string StartHint = "Esc pauses the fire. The fire is very patient.";
        private const string PauseHint = "Esc works too. We're flexible.";
        private const float TitleRestDegrees = -2f;

        private static readonly SwapInterval[] Modes =
        {
            SwapInterval.Off,
            SwapInterval.Seconds90,
            SwapInterval.Seconds60,
            SwapInterval.Seconds30
        };

        private static readonly string[] ModeCaptions =
        {
            "Nobody swaps you. Suspiciously peaceful.",
            "About every 90 s you wake up as the other firefighter. Oops.",
            "About every 60 s: new body, same problems. Recommended by 4 out of 5 arsonists.",
            "About every 30 s. Who are you? Where is your hose? Nobody knows."
        };

        private readonly VisualElement _backdrop;
        private readonly VisualElement _menu;
        private readonly VisualElement _logo;
        private readonly VisualElement _card;
        private readonly Label _title;
        private readonly Label _tagline;
        private readonly Label _hint;
        private readonly Label _modeCaption;
        private readonly Label _sfxValue;
        private readonly Label _musicValue;
        private readonly Slider _sfx;
        private readonly Slider _music;
        private readonly Button _play;
        private readonly Button[] _modeButtons;
        private Sequence _open;
        private Tween _close;
        private Tween _logoPulse;
        private Tween _playPulse;
        private Tween _modePop;
        private Tween _captionShake;
        private Tween _sfxPop;
        private Tween _musicPop;

        public bool IsShown { get; private set; }

        public event Action PlayClicked;
        public event Action<SwapInterval> ModeSelected;
        public event Action<float> SfxVolumeChanged;
        public event Action<float> MusicVolumeChanged;

        public MenuScreen(VisualElement root)
        {
            _backdrop = root.Q("menuBackdrop");
            _menu = root.Q("menu");
            _logo = root.Q("menuLogo");
            _card = root.Q("menuCard");
            _title = root.Q<Label>("menuTitle");
            _tagline = root.Q<Label>("menuTagline");
            _hint = root.Q<Label>("menuHint");
            _modeCaption = root.Q<Label>("modeCaption");
            _sfxValue = root.Q<Label>("sfxValue");
            _musicValue = root.Q<Label>("musicValue");
            _sfx = root.Q<Slider>("sfxSlider");
            _music = root.Q<Slider>("musicSlider");
            _play = root.Q<Button>("playButton");
            _modeButtons = new[] { root.Q<Button>("modeOff"), root.Q<Button>("mode90"), root.Q<Button>("mode60"), root.Q<Button>("mode30") };

            for (var index = 0; index < _modeButtons.Length; index++)
            {
                var mode = Modes[index];
                _modeButtons[index].focusable = false;
                _modeButtons[index].clicked += () => OnModeClicked(mode);
            }

            _play.focusable = false;
            _play.clicked += () => PlayClicked?.Invoke();
            _sfx.RegisterValueChangedCallback(evt => OnSfxChanged(evt.newValue));
            _music.RegisterValueChangedCallback(evt => OnMusicChanged(evt.newValue));
        }

        public void Show(bool isPause, MenuSettings settings)
        {
            IsShown = true;
            _play.text = isPause ? ResumeText : PlayText;
            _tagline.text = isPause ? PauseTagline : StartTagline;
            _hint.text = isPause ? PauseHint : StartHint;
            ShowMode(settings.Swap, false);
            _sfx.SetValueWithoutNotify(settings.SfxVolume);
            _music.SetValueWithoutNotify(settings.MusicVolume);
            _sfxValue.text = PercentText(settings.SfxVolume);
            _musicValue.text = PercentText(settings.MusicVolume);

            _close?.Kill();
            _backdrop.RemoveFromClassList(HiddenClass);
            _menu.RemoveFromClassList(HiddenClass);
            _menu.style.opacity = 1f;
            UiTween.Restart(ref _logoPulse, null);
            UiTween.Restart(ref _playPulse, null);
            _open?.Kill();
            _open = DOTween.Sequence()
                .Insert(0f, UiTween.Fade(_backdrop, 0f, 1f, 0.2f, Ease.OutQuad))
                .Insert(0f, UiTween.SlideIn(_title, new Vector2(0f, -240f), 0.55f, Ease.OutBounce))
                .Insert(0.3f, UiTween.Wobble(_title, 14f, TitleRestDegrees, 0.9f))
                .Insert(0.1f, UiTween.Pop(_logo, 0f, 0.6f, Ease.OutElastic))
                .Insert(0.15f, UiTween.SlideIn(_card, new Vector2(0f, 80f), 0.4f))
                .Insert(0.15f, UiTween.Fade(_card, 0f, 1f, 0.25f, Ease.OutQuad))
                .Insert(0.35f, UiTween.Pop(_play, 0f, 0.45f))
                .AppendCallback(StartIdleLoops)
                .SetUpdate(true);
        }

        public void Hide()
        {
            IsShown = false;
            _open?.Kill();
            UiTween.Restart(ref _logoPulse, null);
            UiTween.Restart(ref _playPulse, null);
            _close?.Kill();
            _close = DOTween.Sequence()
                .Insert(0f, UiTween.Squash(_play, new Vector2(1.3f, 0.7f), 0.2f))
                .Insert(0f, UiTween.Fade(_menu, 1f, 0f, 0.2f, Ease.InQuad))
                .Insert(0f, UiTween.Fade(_backdrop, 1f, 0f, 0.2f, Ease.InQuad))
                .OnComplete(() =>
                {
                    _backdrop.AddToClassList(HiddenClass);
                    _menu.AddToClassList(HiddenClass);
                })
                .SetUpdate(true);
        }

        public void Dispose()
        {
            _open?.Kill();
            _close?.Kill();
            _logoPulse?.Kill();
            _playPulse?.Kill();
            _modePop?.Kill();
            _captionShake?.Kill();
            _sfxPop?.Kill();
            _musicPop?.Kill();
        }

        private void StartIdleLoops()
        {
            UiTween.Restart(ref _logoPulse, UiTween.Pulse(_logo, 1.05f, 1.1f));
            UiTween.Restart(ref _playPulse, UiTween.Pulse(_play, 1.06f, 0.5f));
        }

        private void OnModeClicked(SwapInterval mode)
        {
            ShowMode(mode, true);
            ModeSelected?.Invoke(mode);
        }

        private void ShowMode(SwapInterval mode, bool animate)
        {
            var selected = Array.IndexOf(Modes, mode);
            for (var index = 0; index < _modeButtons.Length; index++)
                _modeButtons[index].EnableInClassList(SelectedClass, index == selected);

            _modeCaption.text = ModeCaptions[selected];
            if (!animate) return;

            UiTween.Restart(ref _modePop, UiTween.Squash(_modeButtons[selected], new Vector2(1.25f, 0.8f), 0.5f, Ease.OutElastic));
            UiTween.Restart(ref _captionShake, UiTween.Shake(_modeCaption, 6f, 0.3f));
        }

        private void OnSfxChanged(float fraction)
        {
            _sfxValue.text = PercentText(fraction);
            UiTween.Restart(ref _sfxPop, UiTween.Pop(_sfxValue, 1.25f, 0.2f));
            SfxVolumeChanged?.Invoke(fraction);
        }

        private void OnMusicChanged(float fraction)
        {
            _musicValue.text = PercentText(fraction);
            UiTween.Restart(ref _musicPop, UiTween.Pop(_musicValue, 1.25f, 0.2f));
            MusicVolumeChanged?.Invoke(fraction);
        }

        private static string PercentText(float fraction) => $"{Mathf.RoundToInt(fraction.ToPercent())}%";
    }
}
