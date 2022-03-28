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
            InitIfLocalPlayerIsAlreadySpawned();
            
            action = actionAtStart.action;
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
            }
            PlayerEntity.OnPlayerSpawned -= Init;
        }

        private void Init(NetworkObject player)
        {
            if (!player || !player.HasInputAuthority)
                return;

            playerInputHandler = player.GetComponentInChildren<PlayerInputHandler>();
            Debug.Assert(playerInputHandler);

            playerInputHandler.OnInputDeviceChanged += OnInputDeviceChanged;
            inputDevice = playerInputHandler.CurrentDevice;

            UpdateIcon();
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