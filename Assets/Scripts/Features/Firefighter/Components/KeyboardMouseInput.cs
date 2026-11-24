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
            return new FirefighterCommands(move, mouse.delta.ReadValue(), keyboard.spaceKey.wasPressedThisFrame, keyboard.eKey.wasPressedThisFrame,
                mouse.leftButton.wasPressedThisFrame);
        }

        private static float Axis(KeyControl negative, KeyControl positive) => 
            (positive.isPressed ? 1f : 0f) - (negative.isPressed ? 1f : 0f);
    }
}
