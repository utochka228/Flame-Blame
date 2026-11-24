using System;
using System.Collections.Generic;
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

    [Serializable]
    public struct HoseJoint
    {
        public const int CouplingEndCount = 2;
        public const int SplitterEndCount = 3;

        [SerializeField] private HoseEndRef[] _ends;

        public IReadOnlyList<HoseEndRef> Ends => _ends;
        public bool IsSplitter => _ends.Length == SplitterEndCount;

        public HoseJoint(params HoseEndRef[] ends)
        {
            _ends = ends;
        }
    }
}
