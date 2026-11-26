using System;
using DG.Tweening;
using FlameBlame.Firefighter.Simulation;
using FlameBlame.UI.Kit;
using UnityEngine;
using UnityEngine.UIElements;

namespace FlameBlame.UI.Hud
{
    public class InteractionPromptView : IDisposable
    {
        private readonly VisualElement _prompt;
        private readonly Label _key;
        private readonly Label _verb;
        private readonly Label _target;
        private UseAction _shown;
        private Tween _fade;
        private Tween _motion;
        private Tween _keyPop;

        public InteractionPromptView(VisualElement root)
        {
            _prompt = root.Q("prompt");
            _key = root.Q<Label>("promptKey");
            _verb = root.Q<Label>("promptVerb");
            _target = root.Q<Label>("promptTarget");
        }

        public void Show(UseAction action)
        {
            if (action == _shown) return;

            var wasHidden = _shown == UseAction.None;
            _shown = action;
            if (action == UseAction.None)
            {
                Hide();
                return;
            }

            _verb.text = Verb(action);
            _target.text = Target(action);
            if (wasHidden)
                Appear();
            else
                UiTween.Restart(ref _motion, UiTween.Squash(_prompt, new Vector2(0.9f, 1.1f), 0.25f));
        }

        public void Press()
        {
            UiTween.Restart(ref _keyPop, UiTween.Pop(_key, 0.7f, 0.25f));
        }

        public void Dispose()
        {
            _fade?.Kill();
            _motion?.Kill();
            _keyPop?.Kill();
        }

        private void Appear()
        {
            UiTween.Restart(ref _fade, UiTween.Fade(_prompt, _prompt.resolvedStyle.opacity, 1f, 0.15f, Ease.OutCubic));
            UiTween.Restart(ref _motion, UiTween.SlideIn(_prompt, new Vector2(0f, 12f), 0.18f, Ease.OutCubic));
            UiTween.Restart(ref _keyPop, UiTween.Pop(_key, 0.5f, 0.3f));
        }

        private void Hide()
        {
            UiTween.Restart(ref _fade, UiTween.Fade(_prompt, _prompt.resolvedStyle.opacity, 0f, 0.1f, Ease.InQuad));
        }

        private static string Verb(UseAction action)
        {
            switch (action)
            {
                case UseAction.Connect: return "Connect";
                case UseAction.Tape: return "Tape";
                case UseAction.Disconnect: return "Disconnect";
                case UseAction.ToggleValve: return "Turn";
                case UseAction.Tie: return "Tie";
                case UseAction.Untie: return "Untie";
                case UseAction.Climb: return "Climb";
                case UseAction.StopClimb: return "Let go";
                case UseAction.ToggleDoor: return "Use";
                case UseAction.Latch: return "Latch";
                case UseAction.Unlatch: return "Unlatch";
                default: return string.Empty;
            }
        }

        private static string Target(UseAction action)
        {
            switch (action)
            {
                case UseAction.Connect:
                case UseAction.Tie:
                case UseAction.Untie:
                case UseAction.Climb:
                    return "Hose";
                case UseAction.Tape: return "Burst";
                case UseAction.Disconnect: return "Coupling";
                case UseAction.ToggleValve: return "Valve";
                case UseAction.ToggleDoor: return "Door";
                case UseAction.Latch:
                case UseAction.Unlatch:
                    return "Socket";
                default: return string.Empty;
            }
        }
    }
}
