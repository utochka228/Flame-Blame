namespace FlameBlame.Firefighter.Simulation
{
    public class SlipGate
    {
        private float _minSpeed;
        private float _fallSeconds;
        private float _immunitySeconds;
        private float _sinceSlipped = float.PositiveInfinity;

        public bool IsFallen => _sinceSlipped < _fallSeconds;

        public SlipGate(float minSpeed, float fallSeconds, float immunitySeconds)
        {
            Configure(minSpeed, fallSeconds, immunitySeconds);
        }

        public void Configure(float minSpeed, float fallSeconds, float immunitySeconds)
        {
            _minSpeed = minSpeed;
            _fallSeconds = fallSeconds;
            _immunitySeconds = immunitySeconds;
        }

        public bool Step(bool isWet, float planarSpeed, float deltaTime)
        {
            _sinceSlipped += deltaTime;
            if (!isWet || planarSpeed < _minSpeed || _sinceSlipped < _fallSeconds + _immunitySeconds)
                return false;

            _sinceSlipped = 0f;
            return true;
        }
    }
}
