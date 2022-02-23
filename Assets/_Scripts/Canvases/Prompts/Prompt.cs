using System.Threading.Tasks;
using Canvases.Components;
using Fusion;
using Sirenix.OdinInspector;
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
        [SerializeField, Tooltip("Action assigned to the prompt at start"), ValidateInput(nameof(ValidateActionAtStart), "Action is not valid. See class PlayerActionHandler to know valid actions")] 
        private string actionAtStart = "";
        
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
            
            Action = actionAtStart;
            UpdateIcon();
        }

        private InputAction FindActionForActionName(string actionName)
        {
            if (!playerInputHandler)
                return null;
            
            var actionFound = playerInputHandler.GetInputAction(actionName);
            if (actionFound == null)
                Debug.LogWarning($"Tried to find action {actionName} but could not find it. The prompt will not be shown");
            
            return actionFound;
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

        private bool ValidateActionAtStart()
        {
            return PlayerInputHandler.ValidActions.Contains(actionAtStart.ToLower());
        }
    }
}