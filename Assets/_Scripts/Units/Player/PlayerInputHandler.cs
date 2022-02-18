using System;
using Fusion;
using InputSystem;
using Systems.Network;
using UnityEngine;
using UnityEngine.InputSystem;
using Utilities.Extensions;

namespace Units.Player
{
    public class PlayerInputHandler : MonoBehaviour
    {
        public event Action<string> OnInputDeviceChanged
        {
            add => detectDevice.OnInputDeviceChanged += value;
            remove => detectDevice.OnInputDeviceChanged -= value;
        }

        public string CurrentDevice => detectDevice.CurrentDevice;

        public PlayerInputAction PlayerInputAction { get; private set; }
        private DetectDevice detectDevice;

        [HideInInspector] public InputAction Move;
        [HideInInspector] public InputAction Look;
        [HideInInspector] public InputAction Jump;
        [HideInInspector] public InputAction Attack;
        [HideInInspector] public InputAction AltAttack;
        [HideInInspector] public InputAction Dash;
        [HideInInspector] public InputAction Sprint;
        [HideInInspector] public InputAction Interact;

        private void Awake()
        {
            PlayerInputAction = new PlayerInputAction();
            detectDevice = new DetectDevice(PlayerInputAction.Player.Get());
            RebindSaveLoad.LoadOverrides(PlayerInputAction.asset);
        }

        private void Start() => NetworkSystem.Instance.OnInputEvent += OnInput;
        private void OnInput(NetworkRunner runner, NetworkInput input)
        {
            NetworkInputData data = new NetworkInputData();

            if (Jump.ReadBool()) data.Buttons |= NetworkInputData.BUTTON_JUMP; 
            if (Attack.ReadBool()) data.Buttons |= NetworkInputData.BUTTON_ATTACK; 
            if (AltAttack.ReadBool()) data.Buttons |= NetworkInputData.BUTTON_ALT_ATTACK; 
            if (Dash.ReadBool()) data.Buttons |= NetworkInputData.BUTTON_DASH; 
            if (Sprint.ReadBool()) data.Buttons |= NetworkInputData.BUTTON_SPRINT;
            if (Interact.ReadBool()) data.Buttons |= NetworkInputData.BUTTON_INTERACT;

            data.Move = Move.ReadV2();
            data.Look = Look.ReadV2();
            
            input.Set(data);
        }

        private void OnEnable()
        {
            PlayerInputAction.Enable();
            Move = PlayerInputAction.Player.Movement;
            Look = PlayerInputAction.Player.Look;
            Jump = PlayerInputAction.Player.Jump;
            Attack = PlayerInputAction.Player.Attack;
            AltAttack = PlayerInputAction.Player.AltAttack;
            Dash = PlayerInputAction.Player.Dash;
            Sprint = PlayerInputAction.Player.Sprint;
            Interact = PlayerInputAction.Player.Interact;
        }

        public void SaveSettings() => RebindSaveLoad.SaveOverrides(PlayerInputAction.asset);

        private void OnDisable()
        {
            SaveSettings();
            PlayerInputAction.Disable();
        }
    }
}