using System;
using Fusion;
using InputSystem;
using Systems.Network;
using UnityEngine.InputSystem;
using Utilities.Extensions;

namespace Units.Player
{
    public class PlayerInputHandler : NetworkBehaviour
    {
        public event Action OnInputsRebinded;
        
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
        private InputAction throwing;
        private InputAction ready;
        private InputAction dance;

        private bool interactOnce;
        private bool readyOnce;
        private bool dashOnce;
        private bool menuOnce;
        private bool danceOnce;

        public static bool FetchInput = true;

        public override void Spawned()
        {
            base.Spawned();
            if (Object.HasInputAuthority)
            {
                PlayerInputAction = new PlayerInputAction();
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

            if (FetchInput)
            {
                if (dashOnce) data.Buttons |= NetworkInputData.BUTTON_DASH;
                if (sprint.ReadBool()) data.Buttons |= NetworkInputData.BUTTON_SPRINT;
                if (interactOnce) data.Buttons |= NetworkInputData.BUTTON_INTERACT_ONCE;
                if (menuOnce) data.Buttons |= NetworkInputData.BUTTON_MENU;
                if (throwing.ReadBool()) data.Buttons |= NetworkInputData.BUTTON_THROW;
                if (readyOnce) data.Buttons |= NetworkInputData.BUTTON_READY_ONCE;
                if (danceOnce) data.Buttons |= NetworkInputData.BUTTON_DANCE_ONCE;

                data.Move = move.ReadV2();
            }

            input.Set(data);

            interactOnce = false;
            readyOnce = false;
            dashOnce = false;
            menuOnce = false;
            danceOnce = false;
        }

        private void EnableInput()
        {
            PlayerInputAction.Enable();
            move = PlayerInputAction.Player.Movement;
            dash = PlayerInputAction.Player.Dash;
            sprint = PlayerInputAction.Player.Sprint;
            interact = PlayerInputAction.Player.Interact;
            throwing = PlayerInputAction.Player.Throw;
            ready = PlayerInputAction.Player.Ready;
            
            PlayerInputAction.Player.Dance.started += ActivateDanceOnce;
            PlayerInputAction.Player.Menu.started += ActivateMenuOnce;
            interact.started += ActivateInteractOnce;
            ready.started += ActivateReadyOnce;
            dash.started += ActivateDashOnce;
        }

        private void ActivateInteractOnce(InputAction.CallbackContext ctx) => interactOnce = ctx.started;
        private void ActivateReadyOnce(InputAction.CallbackContext ctx) => readyOnce = ctx.started;
        private void ActivateDashOnce(InputAction.CallbackContext ctx) => dashOnce = ctx.started;
        private void ActivateMenuOnce(InputAction.CallbackContext ctx) => menuOnce = ctx.started;
        private void ActivateDanceOnce(InputAction.CallbackContext ctx) => danceOnce = ctx.started;

        public void SaveSettings()
        {
            if (PlayerInputAction != null)
                RebindSaveLoad.SaveOverrides(PlayerInputAction.asset);
        }

        private void DisposeInputs()
        {
            SaveSettings();
            if (Object.HasInputAuthority)
            {
                PlayerInputAction.Dispose();
            }
        }

        public override void Despawned(NetworkRunner runner, bool hasState)
        {
            base.Despawned(runner, hasState);
            if (NetworkSystem.HasInstance)
                NetworkSystem.Instance.OnInputEvent -= OnInput;
            DisposeInputs();
            detectDevice?.FinalizeInit();
        }

        public void UpdateBindings()
        {
            OnInputsRebinded?.Invoke();
        }
    }
}