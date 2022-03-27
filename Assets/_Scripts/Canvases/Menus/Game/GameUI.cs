using Canvases.Components;
using Sirenix.OdinInspector;
using Systems.Network;
using Units.Player;
using UnityEngine;
using Utilities;

namespace Canvases.Menu.Game
{
    public class GameUI : AbstractMenu
    {
        [SerializeField, Required] private ButtonUIComponent resumeButton;
        [SerializeField, Required] private ButtonUIComponent optionsButton;
        [SerializeField, Required] private ButtonUIComponent mainMenuButton;
        [SerializeField, Required] private ButtonUIComponent exitGameButton;

        private PlayerEntity player;
        
        protected override EntryDirection EnterDirection => EntryDirection.Down;
        protected override EntryDirection LeaveDirection => EntryDirection.Down;

        public override bool ShowForImplementation(PlayerEntity playerEntity)
        {
            player = playerEntity;
            return true;
        }

        public override bool HideImplementation()
        {
            player = null;
            return true;
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            resumeButton.OnClick += OnResumePressed;
            optionsButton.OnClick += OnOptionsPressed;
            mainMenuButton.OnClick += OnMainMenuPressed;
            exitGameButton.OnClick += OnExitPressed;
        }

        private void OnResumePressed()
        {
            if (player)
                player.CloseMenu();
            else
                Hide();
        }

        private void OnOptionsPressed()
        {
            if (MenuManager.HasInstance)
            {
                MenuManager.Instance.ShowMenu(MenuManager.Menu.Options);
            }
        }

        private void OnMainMenuPressed()
        {
            NetworkSystem.Instance.Disconnect();
            // TODO Return to main menu
        }

        private void OnExitPressed()
        {
            NetworkSystem.Instance.Disconnect();
            ExitHelper.ExitGame();
        }
    }
}