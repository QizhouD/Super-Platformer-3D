using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using static PlayerInputActions;

namespace Platformer {
    [CreateAssetMenu(fileName = "InputReader", menuName = "Platformer/InputReader")]
    public class InputReader : ScriptableObject, IPlayerActions {
        public event UnityAction<Vector2> Move = delegate { };
        public event UnityAction<Vector2, bool> Look = delegate { };
        public event UnityAction EnableMouseControlCamera = delegate { };
        public event UnityAction DisableMouseControlCamera = delegate { };
        public event UnityAction<bool> Jump = delegate { };
        public event UnityAction<bool> Dash = delegate { };
        public event UnityAction Attack = delegate { };
        public event UnityAction Pause = delegate { };

        PlayerInputActions inputActions;
        Vector2 externalDirection;

        public bool ExternalControlEnabled { get; private set; }
        public bool HumanJumpHeld { get; private set; }
        public bool HumanDashHeld { get; private set; }
        public Vector2 HumanDirection =>
            inputActions != null
                ? inputActions.Player.Move.ReadValue<Vector2>()
                : Vector2.zero;
        public Vector3 Direction =>
            ExternalControlEnabled ? externalDirection : HumanDirection;

        void OnEnable() {
            if (inputActions == null) {
                inputActions = new PlayerInputActions();
                inputActions.Player.SetCallbacks(this);
            }
        }

        public void EnablePlayerActions() {
            inputActions.Enable();
        }

        public void SetExternalControlEnabled(bool value) {
            if (ExternalControlEnabled == value) return;
            if (ExternalControlEnabled) {
                Jump.Invoke(false);
                Dash.Invoke(false);
            }
            ExternalControlEnabled = value;
            externalDirection = Vector2.zero;
        }

        public void SetExternalDirection(Vector2 direction) {
            externalDirection = Vector2.ClampMagnitude(direction, 1f);
        }

        public void SendExternalJump(bool pressed) {
            if (ExternalControlEnabled) Jump.Invoke(pressed);
        }

        public void SendExternalDash(bool pressed) {
            if (ExternalControlEnabled) Dash.Invoke(pressed);
        }

        public void OnMove(InputAction.CallbackContext context) {
            Move.Invoke(context.ReadValue<Vector2>());
        }

        public void OnLook(InputAction.CallbackContext context) {
            Look.Invoke(context.ReadValue<Vector2>(), IsDeviceMouse(context));
        }

        bool IsDeviceMouse(InputAction.CallbackContext context) =>
            context.control.device.name == "Mouse";

        public void OnFire(InputAction.CallbackContext context) {
            if (context.phase == InputActionPhase.Started && !ExternalControlEnabled)
                Attack.Invoke();
        }

        public void OnMouseControlCamera(InputAction.CallbackContext context) {
            switch (context.phase) {
                case InputActionPhase.Started:
                    EnableMouseControlCamera.Invoke();
                    break;
                case InputActionPhase.Canceled:
                    DisableMouseControlCamera.Invoke();
                    break;
            }
        }

        public void OnRun(InputAction.CallbackContext context) {
            switch (context.phase) {
                case InputActionPhase.Started:
                    HumanDashHeld = true;
                    if (!ExternalControlEnabled) Dash.Invoke(true);
                    break;
                case InputActionPhase.Canceled:
                    HumanDashHeld = false;
                    if (!ExternalControlEnabled) Dash.Invoke(false);
                    break;
            }
        }

        public void OnJump(InputAction.CallbackContext context) {
            switch (context.phase) {
                case InputActionPhase.Started:
                    HumanJumpHeld = true;
                    if (!ExternalControlEnabled) Jump.Invoke(true);
                    break;
                case InputActionPhase.Canceled:
                    HumanJumpHeld = false;
                    if (!ExternalControlEnabled) Jump.Invoke(false);
                    break;
            }
        }

        public void OnPause(InputAction.CallbackContext context) {
            if (context.phase == InputActionPhase.Started) Pause.Invoke();
        }
    }
}
