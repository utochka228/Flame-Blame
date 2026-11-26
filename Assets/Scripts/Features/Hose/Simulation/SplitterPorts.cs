using System;

namespace FlameBlame.Hose.Simulation
{
    public class SplitterPorts
    {
        private readonly RopeEnd?[] _ends;

        public SplitterPorts(int portCount)
        {
            _ends = new RopeEnd?[portCount];
        }

        public int PortCount => _ends.Length;

        public int PluggedCount { get; private set; }

        public bool IsPlugged(int port) => _ends[port].HasValue;

        public RopeEnd EndAt(int port) => _ends[port].Value;

        public bool Plug(int port, RopeEnd end)
        {
            if (IsPlugged(port) || Contains(end)) return false;

            _ends[port] = end;
            PluggedCount++;
            return true;
        }

        public void Unplug(int port)
        {
            _ends[port] = null;
            PluggedCount--;
        }

        public bool Contains(RopeEnd end) => Array.IndexOf(_ends, end) >= 0;
    }
}
