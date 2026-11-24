using UnityEngine;

namespace FlameBlame.Firefighter.Simulation
{
    public readonly struct FirefighterCommands
    {
        public Vector2 Move { get; }
        public Vector2 Look { get; }
        public bool JumpPressed { get; }

        public FirefighterCommands(Vector2 move, Vector2 look, bool jumpPressed)
        {
            Move = Vector2.ClampMagnitude(move, 1f);
            Look = look;
            JumpPressed = jumpPressed;
        }
    }
}
