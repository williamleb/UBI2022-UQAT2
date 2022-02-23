using System;
using System.Collections.Generic;
using Fusion;
using InputSystem;
using Systems.Network;
using UnityEngine.InputSystem;
using Utilities.Extensions;

namespace Units.Player
{
    public class PlayerInputHandler : NetworkBehaviour
    {
        public event Action<string> OnInputDeviceChanged
        {
            add => detectDevice.OnInputDeviceChanged += value;
            remove => detectDevice.OnInputDeviceChanged -= value;
        }

        public string CurrentDevice => detectDevice.CurrentDevice;

        public PlayerInputAction PlayerInputAction { get; private set; }
        private DetectDevice detectDevice;

        private InputAction move;
        private InputAction dash;
        private InputAction sprint;
        private InputAction interact;

        private bool interactOnce;
        private bool dashOnce;
        
        public static List<string> ValidActions => new List<string>()
        {
            nameof(move),
            nameof(dash),
            nameof(sprint),
            nameof(interact)
        };
        
        public InputAction GetInputAction(string inputActionName) => inputActionName.ToLower() switch
        {
            nameof(move) => move,
            nameof(dash) => dash,
            nameof(sprint) => sprint,
            nameof(interact) => interact,
            _ => null
        };

        public override void Spawned()
        {
            base.Spawned();

            if (Object.HasInputAuthority)
            {
                PlayerInputAction = new PlayerInputAction();
                detectDevice = new DetectDevice(PlayerInputAction.Player.Get());
                RebindSaveLoad.LoadOverrides(PlayerInputAction.asset);
                EnableInput();
                NetworkSystem.Instance.OnInputEvent += OnInput;
            }
        }

        private void OnInput(NetworkRunner runner, NetworkInput input)
        {
            NetworkInputData data = new NetworkInputData();
            
            if (dashOnce) data.Buttons |= NetworkInputData.BUTTON_DASH;
            if (sprint.ReadBool()) data.Buttons |= NetworkInputData.BUTTON_SPRINT;
            if (interact.ReadBool()) data.Buttons |= NetworkInputData.BUTTON_INTERACT;
            if (interactOnce) data.Buttons |= NetworkInputData.BUTTON_INTERACT_ONCE;

            interactOnce = false;
            dashOnce = false;

            var moveValue = move.ReadV2();
            if (moveValue.x > 0.1) data.Buttons |= NetworkInputData.BUTTON_RIGHT;
            if (moveValue.x < -0.1) data.Buttons |= NetworkInputData.BUTTON_LEFT;
            if (moveValue.y > 0.1) data.Buttons |= NetworkInputData.BUTTON_UP;
            if (moveValue.y < -0.1) data.Buttons |= NetworkInputData.BUTTON_DOWN;

            input.Set(data);
        }

        private void EnableInput()
        {
            PlayerInputAction.Enable();
            move = PlayerInputAction.Player.Movement;
            dash = PlayerInputAction.Player.Dash;
            sprint = PlayerInputAction.Player.Sprint;
            interact = PlayerInputAction.Player.Interact;

            interact.started += ActivateInteractOnce;
            dash.started += ActivateDashOnce;
        }

        private void ActivateInteractOnce(InputAction.CallbackContext ctx) => interactOnce = ctx.started;
        private void ActivateDashOnce(InputAction.CallbackContext ctx) => dashOnce = ctx.started;

        public void SaveSettings() => RebindSaveLoad.SaveOverrides(PlayerInputAction.asset);

        private void OnDestroy() => DisposeInputs();

        private void DisposeInputs()
        {
            SaveSettings();
            PlayerInputAction.Dispose();
        }

        public override void Despawned(NetworkRunner runner, bool hasState)
        {
            base.Despawned(runner, hasState);
            DisposeInputs();
        }
    }
}