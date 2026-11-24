using UnityEngine;

namespace FlameBlame.Firefighter.Simulation
{
    public readonly struct FirefighterCommands
    {
        public Vector2 Move { get; }
        public Vector2 Look { get; }
        public bool JumpPressed { get; }
        public bool GrabPressed { get; }
        public bool ThrowPressed { get; }

        public FirefighterCommands(Vector2 move, Vector2 look, bool jumpPressed, bool grabPressed, bool throwPressed)
        {
            Move = Vector2.ClampMagnitude(move, 1f);
            Look = look;
            JumpPressed = jumpPressed;
            GrabPressed = grabPressed;
            ThrowPressed = throwPressed;
        }
    }
}
