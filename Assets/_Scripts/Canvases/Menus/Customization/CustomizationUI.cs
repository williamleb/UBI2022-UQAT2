using Canvases.Components;
using Canvases.EntryAnimations;
using Sirenix.OdinInspector;
using Units.Player;
using UnityEngine;

namespace Canvases.Menu.Customization
{
    public class CustomizationUI : AbstractMenu
    {

        [SerializeField, Required] private ButtonUIComponent backButton;

        private CustomizationUIElement[] customizationUIElements;

        private PlayerEntity player;

        protected override EntryDirection EnterDirection => EntryDirection.Down;
        protected override EntryDirection LeaveDirection => EntryDirection.Down;

        protected override void Awake()
        {
            base.Awake();
            customizationUIElements = GetComponentsInChildren<CustomizationUIElement>();
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            backButton.OnClick += OnBack;
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            backButton.OnClick += OnBack;
        }

        private void OnBack()
        {
            player.StopCustomization();
            Hide();
        }
        
        public override void Show()
        {
            // Cannot show the customization UI if it's not associated to a player
            return;
        }

        public override void ShowForImplementation(PlayerEntity playerEntity)
        {
            player = playerEntity;
            foreach (var element in customizationUIElements)
            {
                element.Activate(playerEntity);
            }
        }

        public override void HideImplementation()
        {
            foreach (var element in customizationUIElements)
            {
                element.Deactivate();
            }
        }
    }
}