using UnityEngine;

namespace FlameBlame.Firefighter.Simulation
{
    public readonly struct FirefighterCommands
    {
        private readonly FirefighterButtons _pressed;
        private readonly FirefighterButtons _held;

        public Vector2 Move { get; }
        public Vector2 Look { get; }
        public int AdjustSteps { get; }

        public FirefighterCommands(Vector2 move, Vector2 look, FirefighterButtons pressed, FirefighterButtons held, int adjustSteps = 0)
        {
            Move = Vector2.ClampMagnitude(move, 1f);
            Look = look;
            AdjustSteps = adjustSteps;
            _pressed = pressed;
            _held = held;
        }

        public bool WasPressed(FirefighterButtons button) => (_pressed & button) != 0;

        public bool IsHeld(FirefighterButtons button) => (_held & button) != 0;
    }
}
