using FlameBlame.Hose.Components;
using UnityEngine;

namespace FlameBlame.Hose.Presentation
{
    public class HoseSplitterView : MonoBehaviour
    {
        [SerializeField] private HoseNetwork _network;
        [SerializeField] private int _jointIndex;

        private void LateUpdate()
        {
            transform.position = _network.JointPosition(_jointIndex);
        }
    }
}
