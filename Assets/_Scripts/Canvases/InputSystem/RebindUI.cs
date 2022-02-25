using System.Collections.Generic;
using Canvases.Components;
using Fusion;
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

        private PlayerInputAction playerInputActionRef;
        private PlayerInputHandler playerInputHandler;

        private readonly List<RebindActionUI> rebindUIs = new List<RebindActionUI>();

        private void Awake() => PlayerEntity.OnPlayerSpawned += Init;

        private void Init(NetworkObject player)
        {
            if (player.HasInputAuthority)
            {
                playerInputHandler = player.GetComponent<PlayerInputHandler>();
                player.GetComponent<PlayerEntity>().OnMenuPressed += PauseMenuActionOnStarted;
                playerInputActionRef = playerInputHandler.PlayerInputAction;
                resetAllButton.OnClick += OnResetAll;
                Enable();
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

        private void Enable()
        {
            if (playerInputHandler != null)
                playerInputHandler.OnInputDeviceChanged += OnInputDeviceChanged;
        }

        private void OnInputDeviceChanged(string newDevice)
        {
            rebindUIs.Clear();
            scrollRectContent.DestroyChildren();
            AddBindingsButton(newDevice);
        }

        private void OnDestroy()
        {
            if (playerInputHandler != null)
                playerInputHandler.OnInputDeviceChanged -= OnInputDeviceChanged;
        }

        private void PauseMenuActionOnStarted()
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
                    OnInputDeviceChanged(playerInputHandler.CurrentDevice);
                }

                Time.timeScale = 0;
            }
        }
    }
}