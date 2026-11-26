namespace FlameBlame.Hose.Simulation
{
    public class SplitterPorts
    {
        private readonly RopeEnd[] _ends;
        private readonly bool[] _isPlugged;

        public SplitterPorts(int portCount)
        {
            _ends = new RopeEnd[portCount];
            _isPlugged = new bool[portCount];
        }

        public int PortCount => _ends.Length;

        public int PluggedCount
        {
            get
            {
                var count = 0;
                foreach (var isPlugged in _isPlugged)
                {
                    if (isPlugged)
                        count++;
                }

                return count;
            }
        }

        public bool IsPlugged(int port) => _isPlugged[port];

        public RopeEnd EndAt(int port) => _ends[port];

        public bool Plug(int port, RopeEnd end)
        {
            if (_isPlugged[port] || Contains(end)) return false;

            _ends[port] = end;
            _isPlugged[port] = true;
            return true;
        }

        public void Unplug(int port)
        {
            _isPlugged[port] = false;
        }

        public bool Contains(RopeEnd end)
        {
            for (var port = 0; port < _ends.Length; port++)
            {
                if (_isPlugged[port] && _ends[port].Rope == end.Rope && _ends[port].Kind == end.Kind)
                    return true;
            }

            return false;
        }
    }
}
