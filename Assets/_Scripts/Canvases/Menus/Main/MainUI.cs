using Canvases.Components;
using Sirenix.OdinInspector;
using UnityEngine;
using Utilities;

namespace Canvases.Menu.Main
{
    public class MainUI : AbstractMenu
    {
        [SerializeField, Required] private ButtonUIComponent hostButton;
        [SerializeField, Required] private ButtonUIComponent joinButton;
        [SerializeField, Required] private ButtonUIComponent optionsButton;
        [SerializeField, Required] private ButtonUIComponent exitButton;

        protected override EntryDirection EnterDirection => EntryDirection.Down;
        protected override EntryDirection LeaveDirection => EntryDirection.Down;

        protected override void OnEnable()
        {
            base.OnEnable();
            hostButton.OnClick += OnHostButtonPressed;
            joinButton.OnClick += OnJoinButtonPressed;
            optionsButton.OnClick += OnOptionsButtonPressed;
            exitButton.OnClick += OnExitButtonPressed;
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            hostButton.OnClick -= OnHostButtonPressed;
            joinButton.OnClick -= OnJoinButtonPressed;
            optionsButton.OnClick -= OnOptionsButtonPressed;
            exitButton.OnClick -= OnExitButtonPressed;
        }

        private void OnHostButtonPressed()
        {
            if (MenuManager.HasInstance)
            {
                Hide();
                MenuManager.Instance.ShowMenu(MenuManager.Menu.Host);
            }
        }
        
        private void OnJoinButtonPressed()
        {
            if (MenuManager.HasInstance)
            {
                Hide();
                MenuManager.Instance.ShowMenu(MenuManager.Menu.Join);
            }
        }
        
        private void OnOptionsButtonPressed()
        {
            if (MenuManager.HasInstance)
            {
                Unfocus();
                MenuManager.Instance.ShowMenu(MenuManager.Menu.Options);
            }
        }
        
        private void OnExitButtonPressed()
        {
            ExitHelper.ExitGame();
        }
    }
}