using System;
using FlameBlame.Hose.Simulation;
using UnityEngine;

namespace FlameBlame.Hose.Components
{
    [Serializable]
    public struct HoseEndRef
    {
        [field: SerializeField] public HoseSegment Hose { get; private set; }
        [field: SerializeField] public RopeEndKind End { get; private set; }

        public HoseEndRef(HoseSegment hose, RopeEndKind end)
        {
            Hose = hose;
            End = end;
        }
    }
}
