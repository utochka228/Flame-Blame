using UnityEngine;

namespace FlameBlame.Firefighter.Components
{
    [RequireComponent(typeof(Collider))]
    public class SwapTimerStartZone : MonoBehaviour
    {
        [SerializeField] private FirefighterSwap _swap;

        private void OnTriggerExit(Collider other)
        {
            if (other.attachedRigidbody && other.attachedRigidbody.TryGetComponent<FirefighterBody>(out _))
                _swap.StartForcedTimer();
        }
    }
}
