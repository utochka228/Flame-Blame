using UnityEngine;

namespace FlameBlame.Firefighter.Simulation
{
    public class FirefighterMotion
    {
        private readonly float _moveSpeed;
        private readonly float _acceleration;
        private readonly float _deceleration;
        private readonly float _jumpSpeed;

        public FirefighterMotion(float moveSpeed, float acceleration, float deceleration, float jumpSpeed)
        {
            _moveSpeed = moveSpeed;
            _acceleration = acceleration;
            _deceleration = deceleration;
            _jumpSpeed = jumpSpeed;
        }

        public Vector3 Step(Vector3 velocity, Vector2 move, Quaternion facing, float deltaTime)
        {
            var target = facing * new Vector3(move.x, 0f, move.y) * _moveSpeed;
            var rate = move.sqrMagnitude > 0f ? _acceleration : _deceleration;
            var horizontal = Vector3.MoveTowards(new Vector3(velocity.x, 0f, velocity.z), target, rate * deltaTime);
            return new Vector3(horizontal.x, velocity.y, horizontal.z);
        }

        public Vector3 Jump(Vector3 velocity) => new(velocity.x, _jumpSpeed, velocity.z);
    }
}
