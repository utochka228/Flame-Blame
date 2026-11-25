namespace FlameBlame.Firefighter.Simulation
{
    public class ClimbMantle
    {
        private const float ClearanceMeters = 0.2f;
        private const float MaxSeconds = 1f;

        private float _topHeight;
        private float _seconds;

        public void Begin(float topHeight)
        {
            _topHeight = topHeight;
            _seconds = 0f;
        }

        public bool Step(float feetHeight, float deltaTime)
        {
            _seconds += deltaTime;
            return feetHeight >= _topHeight + ClearanceMeters || _seconds >= MaxSeconds;
        }
    }
}
