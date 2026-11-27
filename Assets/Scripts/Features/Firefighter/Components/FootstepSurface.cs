using UnityEngine;

namespace FlameBlame.Firefighter.Components
{
    public enum FootstepSurfaceKind
    {
        Default = 0,
        Grass = 1,
        Mud = 2,
        Water = 3
    }

    public class FootstepSurface : MonoBehaviour
    {
        [field: SerializeField] public FootstepSurfaceKind Kind { get; private set; }
    }
}
