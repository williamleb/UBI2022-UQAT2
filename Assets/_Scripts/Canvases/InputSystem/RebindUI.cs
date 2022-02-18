using System.Collections.Generic;
using Canvases.Components;
using Fusion;
using Systems.Network;
using Units.Player;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;
using Utilities.Extensions;

namespace Canvases.InputSystem
{
    public class RebindUI : MonoBehaviour
    {
        [SerializeField] private ImageUIComponent background;
        [SerializeField] private GameObject rebindMenuContent;
        [SerializeField] private TextUIComponent currentControlScheme;
        [SerializeField] private ButtonUIComponent resetAllButton;
        [SerializeField] private RectTransform scrollRectContent;
        [SerializeField] private TextUIComponent rebindOverlayText;
        [SerializeField] private RebindActionUI rebindPrefab;

        //TODO MJ - Change this to a pause menu action in the action map
        [Space] [SerializeField] private InputAction pauseMenuAction;

        private PlayerInputAction playerInputActionRef;
        private PlayerInputHandler playerInputHandler;

        private readonly List<RebindActionUI> rebindUIs = new List<RebindActionUI>();

        private void Awake() => pauseMenuAction.started += PauseMenuActionOnStarted;

        private void Start()
        {
            NetworkSystem.Instance.OnPlayerJoinedEvent += Init;
        }

        private void Init(NetworkRunner networkRunner, PlayerRef playerRef)
        {
            NetworkObject player = networkRunner.GetPlayerObject(playerRef);
            if (player && player.HasInputAuthority)
            {
                playerInputHandler = player.GetComponent<PlayerInputHandler>();
                playerInputHandler.OnInputDeviceChanged += PlayerInputHandlerOnOnInputHandlerDeviceChanged;
                playerInputActionRef = playerInputHandler.PlayerInputAction;
                resetAllButton.OnClick += OnResetAll;
            }
        }

        private void OnResetAll()
        {
            playerInputActionRef.RemoveAllBindingOverrides();
            UpdateAllRebindUI();
        }

        private void AddBindingsButton(string deviceName)
        {
            currentControlScheme.Text = $"< {deviceName.ToUpper()} >";
            ReadOnlyArray<InputAction> inputActions = playerInputActionRef.Player.Get().actions;
            foreach (InputAction inputAction in inputActions)
            {
                if (inputAction.bindings[0].isComposite)
                {
                    if (deviceName == "Gamepad")
                    {
                        SpawnButton(inputAction, inputAction.bindings.Count - 2);
                    }
                    else
                    {
                        for (int i = 1; i < inputAction.bindings.Count - 2; i += 2)
                        {
                            SpawnButton(inputAction, i);
                        }
                    }
                }
                else
                {
                    SpawnButton(inputAction, deviceName == "Gamepad" ? 2 : 0);
                }
            }
        }

        private void SpawnButton(InputAction inputAction, int mainBindingIndex)
        {
            RebindActionUI actionButton = Instantiate(rebindPrefab, scrollRectContent);
            rebindUIs.Add(actionButton);
            actionButton.name = $"Rebind UI for {inputAction.name}";
            actionButton.Initialize(mainBindingIndex, inputAction, rebindOverlayText, OnUpdateBindingUIEvent);
        }

        private void OnUpdateBindingUIEvent(string deviceLayoutName, string mainControlPath) => UpdateAllRebindUI();

        private void UpdateAllRebindUI()
        {
            foreach (RebindActionUI rebindActionUI in rebindUIs)
            {
                rebindActionUI.UpdateBindingDisplay(false);
            }
        }

        private void OnEnable()
        {
            if (playerInputHandler != null)
                playerInputHandler.OnInputDeviceChanged += PlayerInputHandlerOnOnInputHandlerDeviceChanged;
            pauseMenuAction.Enable();
        }

        private void PlayerInputHandlerOnOnInputHandlerDeviceChanged(string newDevice)
        {
            rebindUIs.Clear();
            scrollRectContent.DestroyChildren();
            AddBindingsButton(newDevice);
        }

        private void OnDisable()
        {
            playerInputHandler.OnInputDeviceChanged -= PlayerInputHandlerOnOnInputHandlerDeviceChanged;
            pauseMenuAction.Disable();
        }

        private void PauseMenuActionOnStarted(InputAction.CallbackContext obj)
        {
            if (rebindMenuContent.IsVisible())
            {
                rebindMenuContent.Hide();
                background.Hide();
                Time.timeScale = 1;
                playerInputHandler.SaveSettings();
            }
            else
            {
                rebindMenuContent.Show();
                background.Show();
                if (scrollRectContent.childCount == 0)
                {
                    PlayerInputHandlerOnOnInputHandlerDeviceChanged(playerInputHandler.CurrentDevice);
                }

                Time.timeScale = 0;
            }
        }
    }
}