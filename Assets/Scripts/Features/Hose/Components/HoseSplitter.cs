using FlameBlame.Hose.Simulation;
using UnityEngine;

namespace FlameBlame.Hose.Components
{
    public class HoseSplitter : MonoBehaviour
    {
        public const int PortCount = 4;

        public SplitterPorts Ports { get; } = new SplitterPorts(PortCount);
    }
}
