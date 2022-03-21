using Canvases.Components;
using Canvases.EntryAnimations;
using Sirenix.OdinInspector;
using Units.Player;
using UnityEngine;

namespace Canvases.Menu.Customization
{
    [RequireComponent(typeof(CanvasGroup))]
    public class CustomizationUI : MonoBehaviour, IMenu
    {
        [SerializeField, Required] private EntryAnimation entry;
        [SerializeField, Required] private ButtonUIComponent backButton;
        [SerializeField, Required] private ButtonUIComponent firstButtonToFocus;

        private CanvasGroup canvasGroup;
        private CustomizationUIElement[] customizationUIElements;

        private PlayerEntity player;

        private void Awake()
        {
            canvasGroup = GetComponent<CanvasGroup>();
            customizationUIElements = GetComponentsInChildren<CustomizationUIElement>();
        }

        private void OnEnable()
        {
            entry.OnEntered += OnEntered;
            backButton.OnClick += OnBack;
        }

        private void OnDisable()
        {
            entry.OnEntered -= OnEntered;
            backButton.OnClick += OnBack;
        }

        private void OnBack()
        {
            player.StopCustomization();
            Hide();
        }

        public void ShowFor(PlayerEntity playerEntity)
        {
            if (entry.IsEnteredOrEntering)
                return;

            player = playerEntity;
            foreach (var element in customizationUIElements)
            {
                element.Activate(playerEntity);
            }
            
            entry.EnterDown();
        }
        
        private void OnEntered()
        {
            canvasGroup.interactable = true;
            firstButtonToFocus.Select();
        }

        public void Hide()
        {
            if (entry.IsLeftOrLeaving)
                return;
            
            foreach (var element in customizationUIElements)
            {
                element.Deactivate();
            }

            entry.LeaveDown();
            canvasGroup.interactable = false;
        }
    }
}