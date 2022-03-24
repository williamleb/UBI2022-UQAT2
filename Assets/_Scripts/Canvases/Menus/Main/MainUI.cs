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
        protected override EntryDirection LeaveDirection => EntryDirection.Up;
        
        private void OnHostButtonPressed()
        {
            
        }
        
        private void OnJoinButtonPressed()
        {
            
        }
        
        private void OnOptionsButtonPressed()
        {
            
        }
        
        private void OnExitButtonPressed()
        {
            ExitHelper.ExitGame();
        }
    }
}