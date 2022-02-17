using System;
using Fusion;
using InputSystem;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Units.Player
{
    public class PlayerInputs : MonoBehaviour
    {
        public event Action<string> OnInputDeviceChanged
        {
            add => detectDevice.OnInputDeviceChanged += value;
            remove => detectDevice.OnInputDeviceChanged -= value;
        }

        public string CurrentDevice => detectDevice.CurrentDevice;

        public PlayerInputAction PlayerInputAction { get; private set; }
        private DetectDevice detectDevice;

        public Vector2 Move;
        public Vector2 Look;
        public bool Jump;
        public bool Attack;
        public bool AltAttack;
        public bool Dash;
        public bool Sprint;
        public bool Interact;

        private void Awake()
        {
            PlayerInputAction = new PlayerInputAction();
            detectDevice = new DetectDevice(PlayerInputAction.Player.Get());
            RebindSaveLoad.LoadOverrides(PlayerInputAction.asset);
        }

        private void OnEnable()
        {
            PlayerInputAction.Enable();
            PlayerInputAction.Player.Jump.started += OnJump;
            PlayerInputAction.Player.Jump.canceled += OnJump;
            PlayerInputAction.Player.Attack.started += OnAttack;
            PlayerInputAction.Player.Attack.canceled += OnAttack;
            PlayerInputAction.Player.AltAttack.started += OnAltAttack;
            PlayerInputAction.Player.AltAttack.canceled += OnAltAttack;
            PlayerInputAction.Player.Dash.started += OnDash;
            PlayerInputAction.Player.Dash.canceled += OnDash;
            PlayerInputAction.Player.Sprint.started += OnSprint;
            PlayerInputAction.Player.Sprint.canceled += OnSprint;
            PlayerInputAction.Player.Interact.started += OnInteract;
            PlayerInputAction.Player.Interact.canceled += OnInteract;
        }

        private void OnJump(InputAction.CallbackContext obj) => Jump = obj.started;
        private void OnAttack(InputAction.CallbackContext obj) => Attack = obj.started;
        private void OnAltAttack(InputAction.CallbackContext obj) => AltAttack = obj.started;
        private void OnDash(InputAction.CallbackContext obj) => Dash = obj.started;
        private void OnSprint(InputAction.CallbackContext obj) => Sprint = obj.started;
        private void OnInteract(InputAction.CallbackContext obj) => Interact = obj.started;

        private void Update()
        {
            if (Time.timeScale > 0)
            {
                Move = PlayerInputAction.Player.Movement.ReadValue<Vector2>();
                Look = PlayerInputAction.Player.Look.ReadValue<Vector2>();
            }
            else
            {
                Move = Vector2.zero;
                Look = Vector2.zero;
            }
        }

        public void SaveSettings() => RebindSaveLoad.SaveOverrides(PlayerInputAction.asset);

        private void OnDisable()
        {
            SaveSettings();
            PlayerInputAction.Disable();
        }
    }
}