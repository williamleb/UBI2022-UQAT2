using UnityEngine;
using UnityEngine.InputSystem;

namespace Units.Player
{
    public class PlayerInputs : MonoBehaviour
    {
        private PlayerInputAction playerInputAction;
        
        public Vector2 move;
        public Vector2 look;
        public bool jump;
        public bool attack;
        public bool altAttack;
        public bool dash;
        public bool sprint;

        private void Awake()
        {
            playerInputAction = new PlayerInputAction();
            //Load key bindings
        }

        private void OnEnable()
        {
            playerInputAction.Enable();
            playerInputAction.Player.Jump.started += OnJump;
            playerInputAction.Player.Attack.started += OnAttack;
            playerInputAction.Player.AltAttack.started += OnAltAttack;
            playerInputAction.Player.Dash.started += OnDash;
            playerInputAction.Player.Sprint.started += OnSprint;
            playerInputAction.Player.Jump.canceled += OnJump;
            playerInputAction.Player.Attack.canceled += OnAttack;
            playerInputAction.Player.AltAttack.canceled += OnAltAttack;
            playerInputAction.Player.Dash.canceled += OnDash;
            playerInputAction.Player.Sprint.canceled += OnSprint;
            #if UNITY_EDITOR //replace with custom if flag
            playerInputAction.Player.Jump.started += DebugKeyPress;
            playerInputAction.Player.Attack.started += DebugKeyPress;
            playerInputAction.Player.AltAttack.started += DebugKeyPress;
            playerInputAction.Player.Dash.started += DebugKeyPress;
            playerInputAction.Player.Sprint.started += DebugKeyPress;
            #endif
            
        }

        #if DEBUG
        private void DebugKeyPress(InputAction.CallbackContext obj) => print($"{obj.action} was pressed");
        #endif
        private void OnJump(InputAction.CallbackContext obj) => jump = obj.started;
        private void OnAttack(InputAction.CallbackContext obj) => attack = obj.started;
        private void OnAltAttack(InputAction.CallbackContext obj) => altAttack = obj.started;
        private void OnDash(InputAction.CallbackContext obj) => dash = obj.started;
        private void OnSprint(InputAction.CallbackContext obj) => sprint = obj.started;

        private void Update()
        {
            move = playerInputAction.Player.Movement.ReadValue<Vector2>();
            look = playerInputAction.Player.Look.ReadValue<Vector2>();
        }

        private void OnDisable() => playerInputAction.Disable();
    }
}
