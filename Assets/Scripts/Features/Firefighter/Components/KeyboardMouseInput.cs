using FlameBlame.Firefighter.Simulation;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

namespace FlameBlame.Firefighter.Components
{
    public class KeyboardMouseInput
    {
        public FirefighterCommands Read()
        {
            var keyboard = Keyboard.current;
            var mouse = Mouse.current;
            
            if (keyboard == null || mouse == null) 
                return default;

            var move = new Vector2(Axis(keyboard.aKey, keyboard.dKey), Axis(keyboard.sKey, keyboard.wKey));
            var held = mouse.rightButton.isPressed || keyboard.vKey.isPressed ? FirefighterButtons.Thumb : FirefighterButtons.None;
            if (keyboard.spaceKey.isPressed)
                held |= FirefighterButtons.Jump;
            var scroll = mouse.scroll.ReadValue().y;
            var adjustSteps = scroll > 0f ? 1 : scroll < 0f ? -1 : 0;
            return new FirefighterCommands(move, mouse.delta.ReadValue(), PressedButtons(keyboard, mouse), held, adjustSteps);
        }

        private static FirefighterButtons PressedButtons(Keyboard keyboard, Mouse mouse)
        {
            var pressed = FirefighterButtons.None;
            if (keyboard.spaceKey.wasPressedThisFrame)
                pressed |= FirefighterButtons.Jump;
            if (keyboard.eKey.wasPressedThisFrame)
                pressed |= FirefighterButtons.Grab;
            if (keyboard.fKey.wasPressedThisFrame)
                pressed |= FirefighterButtons.Use;
            if (mouse.leftButton.wasPressedThisFrame)
                pressed |= FirefighterButtons.Throw;

            return pressed;
        }

        private static float Axis(KeyControl negative, KeyControl positive) => 
            (positive.isPressed ? 1f : 0f) - (negative.isPressed ? 1f : 0f);
    }
}
