using FlameBlame.Hose.Components;
using UnityEngine;

namespace FlameBlame.Hose.Presentation
{
    public class NozzleView : MonoBehaviour
    {
        [SerializeField] private Nozzle _nozzle;

        private void LateUpdate()
        {
            transform.SetPositionAndRotation(_nozzle.TipPosition, Quaternion.LookRotation(_nozzle.CurrentDirection));
        }
    }
}
