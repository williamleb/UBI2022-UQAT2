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
        public event Action<string> OnInputDeviceChanged
        {
            add => detectDevice.OnInputDeviceChanged += value;
            remove => detectDevice.OnInputDeviceChanged -= value;
        }

        public string CurrentDevice => detectDevice.CurrentDevice;

        public PlayerInputAction PlayerInputAction { get; private set; }
        private DetectDevice detectDevice;

        private InputAction move;
        private InputAction look;
        private InputAction jump;
        private InputAction attack;
        private InputAction altAttack;
        private InputAction dash;
        private InputAction sprint;
        private InputAction interact;

        private bool interactOnce = false;

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

            if (jump.ReadBool()) data.Buttons |= NetworkInputData.BUTTON_JUMP;
            if (attack.ReadBool()) data.Buttons |= NetworkInputData.BUTTON_ATTACK;
            if (altAttack.ReadBool()) data.Buttons |= NetworkInputData.BUTTON_ALT_ATTACK;
            if (dash.ReadBool()) data.Buttons |= NetworkInputData.BUTTON_DASH;
            if (sprint.ReadBool()) data.Buttons |= NetworkInputData.BUTTON_SPRINT;
            if (interact.ReadBool()) data.Buttons |= NetworkInputData.BUTTON_INTERACT;
            if (interactOnce) data.Buttons |= NetworkInputData.BUTTON_INTERACT_ONCE;

            interactOnce = false;

            data.Move = move.ReadV2();
            data.Look = look.ReadV2();

            input.Set(data);
        }

        private void EnableInput()
        {
            PlayerInputAction.Enable();
            move = PlayerInputAction.Player.Movement;
            look = PlayerInputAction.Player.Look;
            jump = PlayerInputAction.Player.Jump;
            attack = PlayerInputAction.Player.Attack;
            altAttack = PlayerInputAction.Player.AltAttack;
            dash = PlayerInputAction.Player.Dash;
            sprint = PlayerInputAction.Player.Sprint;
            interact = PlayerInputAction.Player.Interact;

            interact.started += ActivateInteractOnce;
        }

        private void ActivateInteractOnce(InputAction.CallbackContext ctx) => interactOnce = ctx.started;

        public void SaveSettings() => RebindSaveLoad.SaveOverrides(PlayerInputAction.asset);

        private void OnDestroy() => DisposeInputs();

        private void DisposeInputs()
        {
            interact.started -= ActivateInteractOnce;
            
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