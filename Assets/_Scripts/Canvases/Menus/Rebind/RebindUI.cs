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

namespace Canvases.Menu.Rebind
{
    public class RebindUI : AbstractMenu
    {
        [SerializeField] private ImageUIComponent background;
        [SerializeField] private GameObject rebindMenuContent;
        [SerializeField] private TextUIComponent currentControlScheme;
        [SerializeField] private ButtonUIComponent resetAllButton;
        [SerializeField] private RectTransform scrollRectContent;
        [SerializeField] private TextUIComponent rebindOverlayText;
        [SerializeField] private RebindActionUI rebindPrefab;
        [SerializeField] private ButtonUIComponent applyButton;

        private PlayerInputAction playerInputActionRef;
        private PlayerInputHandler playerInputHandler;

        private RebindActionUI firstButton;

        private readonly List<RebindActionUI> rebindUIs = new List<RebindActionUI>();
        private bool isInitialized;

        protected override EntryDirection EnterDirection => EntryDirection.Down;
        protected override EntryDirection LeaveDirection => EntryDirection.Down;

        protected override void Awake()
        {
            base.Awake();
            PlayerEntity.OnPlayerSpawned += Init;
            if (GameManager.HasInstance)
                GameManager.Instance.OnGameStateChanged += InitOnGameStart;
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            applyButton.OnClick += OnApplyPressed;
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            applyButton.OnClick -= OnApplyPressed;
        }

        private void OnApplyPressed()
        {
            Hide();
        }

        private void Init(NetworkObject player)
        {
            if (player.HasInputAuthority && !isInitialized)
            {
                isInitialized = true;
                playerInputHandler = player.GetComponentInChildren<PlayerInputHandler>();
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
            if (playerInputHandler) playerInputHandler.UpdateBindings();
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
            }
        }

        private void OnUpdateBindingUIEvent(string deviceLayoutName, string mainControlPath)
        {
            UpdateAllRebindUI();
            if (playerInputHandler) playerInputHandler.UpdateBindings();
        }

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

        protected override bool ShowImplementation()
        {
            if (scrollRectContent.childCount == 0)
            {
                OnInputDeviceChanged(playerInputHandler.CurrentDevice);
            }
            return true;
        }

        protected override bool HideImplementation()
        {
            playerInputHandler.SaveSettings();
            return true;
        }
    }
}