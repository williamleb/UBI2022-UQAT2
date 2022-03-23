using System.Collections.Generic;
using Canvases.Components;
using Fusion;
using Managers.Game;
using Units.Player;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;
using Utilities;
using Utilities.Extensions;
using Utilities.Unity;

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

        private RebindActionUI firstButton;

        private readonly List<RebindActionUI> rebindUIs = new List<RebindActionUI>();

        private void Awake()
        {
            PlayerEntity.OnPlayerSpawned += Init;
            if (GameManager.HasInstance)
                GameManager.Instance.OnGameStateChanged += InitOnGameStart;
        }

        private void Init(NetworkObject player)
        {
            if (player.HasInputAuthority)
            {
                playerInputHandler = player.GetComponentInChildren<PlayerInputHandler>();
                player.GetComponentInChildren<PlayerEntity>().OnMenuPressed += PauseMenuActionOnStarted;
                playerInputActionRef = playerInputHandler.PlayerInputAction;
                resetAllButton.OnClick += OnResetAll;
                Enable();
            }
        }

        private void InitOnGameStart(GameState newState)
        {
            if (newState == GameState.Running)
            {
                GameObject[] players = GameObject.FindGameObjectsWithTag(Tags.PLAYER);
                foreach (GameObject player in players)
                {
                    Init(player.GetComponentInParent<NetworkObject>());
                }
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
            bool first = true;
            foreach (var inputAction in inputActions)
            {
                foreach (var mainBinding in BindingsIconsUtil.GetRelevantMainBindings(inputAction, deviceName))
                {
                    SpawnButton(inputAction, mainBinding, first);
                    first = false;
                }
            }
        }

        private void SpawnButton(InputAction inputAction, int mainBindingIndex, bool first)
        {
            RebindActionUI actionButton = Instantiate(rebindPrefab, scrollRectContent);
            rebindUIs.Add(actionButton);
            actionButton.name = $"Rebind UI for {inputAction.name}";
            actionButton.Initialize(mainBindingIndex, inputAction, rebindOverlayText, OnUpdateBindingUIEvent);
            if (first)
            {
                firstButton = actionButton;
                firstButton.SelectMainBinding();
            }
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
                firstButton.SelectMainBinding();
            }
        }
    }
}