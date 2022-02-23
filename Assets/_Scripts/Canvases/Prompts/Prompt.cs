using System.Threading.Tasks;
using Canvases.Components;
using Fusion;
using Units.Player;
using UnityEngine;
using UnityEngine.InputSystem;
using Utilities;

namespace Canvases.Prompts
{
    [RequireComponent(typeof(ImageUIComponent))]
    [RequireComponent(typeof(CanvasGroup))]
    class Prompt : MonoBehaviour
    {
        [SerializeField] private string debugAction = ""; // TODO Remove
        
        private InputAction action = null;
        private string inputDevice = "";
        
        private PlayerInputHandler playerInputHandler;
        
        private ImageUIComponent image;
        private CanvasGroup canvasGroup;

        public string Action
        {
            set
            {
                action = FindActionForActionName(value);
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
            
            UpdateIcon();
        }

        private void OnDestroy()
        {
            if (playerInputHandler)
            {
                playerInputHandler.OnInputDeviceChanged -= OnInputDeviceChanged;
            }
        }

        private async void Init(NetworkObject player)
        {
            // It's the same hack that is used in RebindUI.Init so we can know when the player is initialized.
            // If we find a better way to know detect that in RebindUI.Init, we should also change it here
            await Task.Delay(100);

            if (!player || !player.HasInputAuthority) 
                return;
            
            playerInputHandler = player.GetComponent<PlayerInputHandler>();
            Debug.Assert(playerInputHandler);

            playerInputHandler.OnInputDeviceChanged += OnInputDeviceChanged;
            inputDevice = playerInputHandler.CurrentDevice;
            
            
            Action = debugAction; // TODO Remove
                
            UpdateIcon();
        }

        private InputAction FindActionForActionName(string actionName)
        {
            if (!playerInputHandler)
                return null;
            
            return playerInputHandler.GetInputAction(actionName);
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