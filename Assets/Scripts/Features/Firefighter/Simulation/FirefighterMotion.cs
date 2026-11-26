using UnityEngine;

namespace FlameBlame.Firefighter.Simulation
{
    public class FirefighterMotion
    {
        private readonly float _moveSpeed;
        private readonly float _acceleration;
        private readonly float _deceleration;
        private readonly float _jumpSpeed;
        private readonly float _airControlFraction;
        private readonly float _fallGravityMultiplier;
        private readonly float _shortJumpGravityMultiplier;

        public FirefighterMotion(float moveSpeed, float acceleration, float deceleration, float jumpSpeed, float airControlFraction = 1f,
            float fallGravityMultiplier = 1f, float shortJumpGravityMultiplier = 1f)
        {
            _moveSpeed = moveSpeed;
            _acceleration = acceleration;
            _deceleration = deceleration;
            _jumpSpeed = jumpSpeed;
            _airControlFraction = airControlFraction;
            _fallGravityMultiplier = fallGravityMultiplier;
            _shortJumpGravityMultiplier = shortJumpGravityMultiplier;
        }

        public Vector3 Step(Vector3 velocity, Vector2 move, Quaternion facing, float deltaTime,
            bool isGrounded = true, float speedScale = 1f, float traction = 1f)
        {
            var target = facing * new Vector3(move.x, 0f, move.y) * (_moveSpeed * speedScale);
            var rate = (move.sqrMagnitude > 0f ? _acceleration : _deceleration) *
                       (isGrounded ? 1f : _airControlFraction) * traction;
            var horizontal = Vector3.MoveTowards(new Vector3(velocity.x, 0f, velocity.z), target, rate * deltaTime);
            return new Vector3(horizontal.x, velocity.y, horizontal.z);
        }

        public Vector3 Jump(Vector3 velocity) => new(velocity.x, _jumpSpeed, velocity.z);

        public Vector3 AddAirGravity(Vector3 velocity, float gravity, bool isJumpHeld, float deltaTime)
        {
            var multiplier = velocity.y < 0f ? _fallGravityMultiplier : isJumpHeld ? 1f : _shortJumpGravityMultiplier;
            return new Vector3(velocity.x, velocity.y + (multiplier - 1f) * gravity * deltaTime, velocity.z);
        }
    }
}
