using System;
using System.Collections.Generic;
using UnityEngine;

namespace FlameBlame.Hose.Components
{
    [Serializable]
    public struct HoseJoint
    {
        private const int SplitterEndCount = 3;

        [SerializeField] private HoseEndRef[] _ends;

        public IReadOnlyList<HoseEndRef> Ends => _ends;
        public bool IsSplitter => _ends.Length == SplitterEndCount;

        public HoseJoint(params HoseEndRef[] ends)
        {
            _ends = ends;
        }
    }
}
