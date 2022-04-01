using Canvases.Components;
using Fusion;
using Systems;
using Units.Player;
using UnityEngine;
using UnityEngine.InputSystem;
using Utilities;

namespace Canvases.Prompts
{
    [RequireComponent(typeof(ImageUIComponent))]
    [RequireComponent(typeof(CanvasGroup))]
    public class Prompt : MonoBehaviour
    {
        [SerializeField, Tooltip("Action assigned to the prompt at start")]
        private InputActionReference actionAtStart;

        private string inputDevice = "";

        private PlayerInputHandler playerInputHandler;

        private ImageUIComponent image;
        private CanvasGroup canvasGroup;
        private InputAction action;
        public InputAction Action
        {
            set
            {
                action = value;
                ReplaceActionByActionFromPlayerInputHandler();
                UpdateIcon();
            }
        }

        private void Awake()
        {
            image = GetComponent<ImageUIComponent>();
            canvasGroup = GetComponent<CanvasGroup>();
        }

        private void Start()
        {
            // We wait for the player to be spawned before having access to the inputs bindings
            // The downside to this is that we cannot show prompts when the player isn't spawned
            PlayerEntity.OnPlayerSpawned += Init;
            
            action = actionAtStart.action;
            InitIfLocalPlayerIsAlreadySpawned();
            
            UpdateIcon();
        }
        
        private void InitIfLocalPlayerIsAlreadySpawned()
        {
            var localPlayer = PlayerSystem.Instance.LocalPlayer;
            if (localPlayer)
            {
                Init(localPlayer.Object);
            }
        }

        private void OnDestroy()
        {
            if (playerInputHandler)
            {
                playerInputHandler.OnInputDeviceChanged -= OnInputDeviceChanged;
                playerInputHandler.OnInputsRebinded -= UpdateIcon;
            }
            PlayerEntity.OnPlayerSpawned -= Init;
        }

        private void Init(NetworkObject player)
        {
            if (!player || !player.HasInputAuthority)
                return;

            playerInputHandler = player.GetComponentInChildren<PlayerInputHandler>();
            Debug.Assert(playerInputHandler);
            
            ReplaceActionByActionFromPlayerInputHandler();

            playerInputHandler.OnInputDeviceChanged += OnInputDeviceChanged;
            playerInputHandler.OnInputsRebinded += UpdateIcon;
            inputDevice = playerInputHandler.CurrentDevice;

            UpdateIcon();
        }

        private void ReplaceActionByActionFromPlayerInputHandler()
        {
            if (!playerInputHandler || action == null)
                return;

            // Only the player input action knows the current customized bindings for the action, which is why we replace our action here 
            foreach (var playerAction in playerInputHandler.PlayerInputAction)
            {
                if (action.name == playerAction.name)
                    action = playerAction;
            }
        }

        private void OnInputDeviceChanged(string device)
        {
            inputDevice = device;
            UpdateIcon();
        }

        private void UpdateIcon()
        {
            var icon = BindingsIconsUtil.GetSprite(action, inputDevice);
            canvasGroup.alpha = icon ? 1f : 0f;
            image.Sprite = icon;
        }
    }
}