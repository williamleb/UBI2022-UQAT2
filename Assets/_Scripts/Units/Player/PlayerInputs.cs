using InputSystem;
using UnityEngine;
using UnityEngine.InputSystem;
using static UnityEngine.InputSystem.InputSystem;

namespace Units.Player
{
    public class PlayerInputs : MonoBehaviour
    {
        private PlayerInputAction playerInputAction;
        private RebindSaveLoad saveLoad;

        public Vector2 move;
        public Vector2 look;
        public bool jump;
        public bool attack;
        public bool altAttack;
        public bool dash;
        public bool sprint;
        public bool isUsingKeyboard = true;
        
        private bool showRebind;

        private void Awake()
        {
            playerInputAction = new PlayerInputAction();
            saveLoad = FindObjectOfType<RebindSaveLoad>();
            saveLoad?.LoadOverrides(playerInputAction.asset);
        }

        private void OnEnable()
        {
            playerInputAction.Enable();
            playerInputAction.Player.Jump.started += OnJump;
            playerInputAction.Player.Jump.canceled += OnJump;
            playerInputAction.Player.Attack.started += OnAttack;
            playerInputAction.Player.Attack.canceled += OnAttack;
            playerInputAction.Player.AltAttack.started += OnAltAttack;
            playerInputAction.Player.AltAttack.canceled += OnAltAttack;
            playerInputAction.Player.Dash.started += OnDash;
            playerInputAction.Player.Dash.canceled += OnDash;
            playerInputAction.Player.Sprint.started += OnSprint;
            playerInputAction.Player.Sprint.canceled += OnSprint;

            playerInputAction.Player.Movement.started += DetectKeyboard;
            playerInputAction.Player.Jump.started += DetectKeyboard;
            playerInputAction.Player.Attack.started += DetectKeyboard;
            playerInputAction.Player.AltAttack.started += DetectKeyboard;
            playerInputAction.Player.Dash.started += DetectKeyboard;
            playerInputAction.Player.Sprint.started += DetectKeyboard;
        }

        private void DetectKeyboard(InputAction.CallbackContext obj)
        {
            isUsingKeyboard = obj.control.device.name == "Keyboard";
            if (isUsingKeyboard)
                EnableDevice(Mouse.current);
            else
                DisableDevice(Mouse.current);
        }

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

        private void OnDisable()
        {
            saveLoad?.SaveOverrides(playerInputAction.asset);
            playerInputAction.Disable();
        }

        private void OnGUI()
        {
            //Allows to display the controls with the associated action.
            //Allows to rebind a control
            
            //TODO rebind vector2D (aka movement)
            //TODO dispose rebindOperation to avoid memory leak
            //TODO save rebind to file and reload on start
            if (!showRebind)
            {
                if (GUI.Button(new Rect(0, 0, 100, 20), "Show rebind")) showRebind = true;
            }
            else
            {
                if (GUI.Button(new Rect(0, 0, 100, 20), "Hide rebind")) showRebind = false;
                var inputActions = playerInputAction.Player.Get().actions;
                for (var index = 0; index < inputActions.Count; index++)
                {
                    GUI.Button(new Rect(0, 20 * (index + 1), 100, 20), inputActions[index].name);
                    var controls = inputActions[index].controls;
                    for (int i = 0; i < controls.Count; i++)
                    {
                        if (GUI.Button(new Rect(30 + 75 * (i + 1), 20 * (index + 1), 75, 20), controls[i].displayName))
                        {
                            playerInputAction.Disable();
                            inputActions[index].PerformInteractiveRebinding(i).Start();
                            playerInputAction.Enable();
                        }
                    }
                }
            }
        }
    }
}