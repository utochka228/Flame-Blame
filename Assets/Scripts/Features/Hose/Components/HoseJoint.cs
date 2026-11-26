using System;
using System.Collections.Generic;
using UnityEngine;

namespace FlameBlame.Hose.Components
{
    [Serializable]
    public struct HoseJoint
    {
        [SerializeField] private HoseEndRef[] _ends;

        public IReadOnlyList<HoseEndRef> Ends => _ends;
        public bool IsSplitter => _ends.Length > 2;

        public HoseJoint(params HoseEndRef[] ends)
        {
            _ends = ends;
        }
    }
}
