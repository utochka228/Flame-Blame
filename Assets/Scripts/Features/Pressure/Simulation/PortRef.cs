namespace FlameBlame.Pressure.Simulation
{
    internal readonly struct PortRef
    {
        public static readonly PortRef None = new PortRef(-1, -1);

        public int Part { get; }
        public int Port { get; }
        public bool IsNone => Part < 0;

        public PortRef(int part, int port)
        {
            Part = part;
            Port = port;
        }
    }
}
