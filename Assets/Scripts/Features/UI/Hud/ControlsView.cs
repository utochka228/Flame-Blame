using System;
using DG.Tweening;
using FlameBlame.Firefighter.Simulation;
using FlameBlame.UI.Kit;
using UnityEngine;
using UnityEngine.UIElements;

namespace FlameBlame.UI.Hud
{
    public class ControlsView : IDisposable
    {
        private const string LitClass = "fb-keycap--lit";

        private readonly ControlKey _move;
        private readonly ControlKey _jump;
        private readonly ControlKey _grab;
        private readonly ControlKey _use;
        private readonly ControlKey _throw;
        private readonly ControlKey _thumb;

        public ControlsView(VisualElement root)
        {
            _move = new ControlKey(root.Q<Label>("controlMove"));
            _jump = new ControlKey(root.Q<Label>("controlJump"));
            _grab = new ControlKey(root.Q<Label>("controlGrab"));
            _use = new ControlKey(root.Q<Label>("controlUse"));
            _throw = new ControlKey(root.Q<Label>("controlThrow"));
            _thumb = new ControlKey(root.Q<Label>("controlThumb"));
        }

        public void Show(FirefighterCommands commands)
        {
            _move.Show(commands.Move != Vector2.zero, false);
            _jump.Show(commands.IsHeld(FirefighterButtons.Jump), commands.WasPressed(FirefighterButtons.Jump));
            _grab.Show(false, commands.WasPressed(FirefighterButtons.Grab));
            _use.Show(false, commands.WasPressed(FirefighterButtons.Use));
            _throw.Show(false, commands.WasPressed(FirefighterButtons.Throw));
            _thumb.Show(commands.IsHeld(FirefighterButtons.Thumb), false);
        }

        public void Dispose()
        {
            _move.Dispose();
            _jump.Dispose();
            _grab.Dispose();
            _use.Dispose();
            _throw.Dispose();
            _thumb.Dispose();
        }

        private sealed class ControlKey : IDisposable
        {
            private readonly Label _label;
            private bool _isHeld;
            private Tween _pop;

            public ControlKey(Label label)
            {
                _label = label;
            }

            public void Show(bool isHeld, bool wasPressed)
            {
                if (wasPressed || (isHeld && !_isHeld))
                {
                    UiTween.Restart(ref _pop, UiTween.Pop(_label, 0.7f, 0.25f));
                }

                _isHeld = isHeld;
                _label.EnableInClassList(LitClass, isHeld || (_pop != null && _pop.IsActive()));
            }

            public void Dispose()
            {
                _pop?.Kill();
            }
        }
    }
}
