using UnityEngine;
using UnityEngine.InputSystem;

namespace FlameBlame.Firefighter.Components
{
    public class FirefighterSwap : MonoBehaviour
    {
        [SerializeField] private FirefighterBody _first;
        [SerializeField] private FirefighterBody _second;

        private readonly KeyboardMouseInput _input = new();

        private bool _firstIsActive = true;

        private void Awake()
        {
            Cursor.lockState = CursorLockMode.Locked;
            ShowActiveEye();
        }

        private void Update()
        {
            if (Keyboard.current != null && Keyboard.current.tabKey.wasPressedThisFrame)
            {
                _firstIsActive = !_firstIsActive;
                ShowActiveEye();
            }

            var commands = _input.Read();
            _first.Drive(_firstIsActive ? commands : default);
            _second.Drive(_firstIsActive ? default : commands);
        }

        private void ShowActiveEye()
        {
            _first.Eye.gameObject.SetActive(_firstIsActive);
            _second.Eye.gameObject.SetActive(!_firstIsActive);
        }
    }
}
