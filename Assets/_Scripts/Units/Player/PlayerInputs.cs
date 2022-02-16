using System;
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
        private RebindSaveLoad saveLoad;
        private DetectDevice detectDevice;

        public Vector2 move;
        public Vector2 look;
        public bool jump;
        public bool attack;
        public bool altAttack;
        public bool dash;
        public bool sprint;

        private void Awake()
        {
            PlayerInputAction = new PlayerInputAction();
            detectDevice = new DetectDevice(PlayerInputAction.Player.Get());
            saveLoad = FindObjectOfType<RebindSaveLoad>();
            if (saveLoad != null)
                saveLoad.LoadOverrides(PlayerInputAction.asset);
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
        }

        private void OnJump(InputAction.CallbackContext obj) => jump = obj.started;
        private void OnAttack(InputAction.CallbackContext obj) => attack = obj.started;
        private void OnAltAttack(InputAction.CallbackContext obj) => altAttack = obj.started;
        private void OnDash(InputAction.CallbackContext obj) => dash = obj.started;
        private void OnSprint(InputAction.CallbackContext obj) => sprint = obj.started;

        private void Update()
        {
            if (Time.timeScale > 0)
            {
                move = PlayerInputAction.Player.Movement.ReadValue<Vector2>();
                look = PlayerInputAction.Player.Look.ReadValue<Vector2>();
            }
            else
            {
                move = Vector2.zero;
                look = Vector2.zero;
            }
        }

        public void SaveSettings()
        {
            if (saveLoad != null)
                saveLoad.SaveOverrides(PlayerInputAction.asset);
        }

        private void OnDisable()
        {
            SaveSettings();
            PlayerInputAction.Disable();
        }
    }
}