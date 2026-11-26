namespace FlameBlame.Firefighter.Simulation
{
    public class JumpGate
    {
        private float _bufferSeconds;
        private float _coyoteSeconds;
        private float _lockoutSeconds;
        private float _sincePressed = float.PositiveInfinity;
        private float _sinceGrounded = float.PositiveInfinity;
        private float _sinceJumped = float.PositiveInfinity;

        public bool IsGrounded { get; private set; }

        public JumpGate(float bufferSeconds, float coyoteSeconds, float lockoutSeconds)
        {
            Configure(bufferSeconds, coyoteSeconds, lockoutSeconds);
        }

        public void Configure(float bufferSeconds, float coyoteSeconds, float lockoutSeconds)
        {
            _bufferSeconds = bufferSeconds;
            _coyoteSeconds = coyoteSeconds;
            _lockoutSeconds = lockoutSeconds;
        }

        public void Press()
        {
            _sincePressed = 0f;
        }

        public void Cancel()
        {
            _sincePressed = float.PositiveInfinity;
            _sinceGrounded = float.PositiveInfinity;
            IsGrounded = false;
        }

        public bool Step(bool isTouchingGround, float deltaTime)
        {
            IsGrounded = isTouchingGround && _sinceJumped >= _lockoutSeconds;
            if (IsGrounded)
                _sinceGrounded = 0f;

            var jumps = _sincePressed <= _bufferSeconds && _sinceGrounded <= _coyoteSeconds;
            if (jumps)
            {
                _sincePressed = float.PositiveInfinity;
                _sinceGrounded = float.PositiveInfinity;
                _sinceJumped = 0f;
                IsGrounded = false;
            }

            _sincePressed += deltaTime;
            _sinceGrounded += deltaTime;
            _sinceJumped += deltaTime;
            return jumps;
        }
    }
}
