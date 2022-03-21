using System;
using Canvases.Menu.Customization;
using Units.Player;
using UnityEngine;
using Utilities.Singleton;

namespace Canvases.Menu
{
    public class MenuManager : Singleton<MenuManager>
    {
        public enum Menu { Customization } 

        private CustomizationUI customizationUI;

        private void Start()
        {
            customizationUI = FindObjectOfType<CustomizationUI>();
        }

        public void ShowMenuForPlayer(Menu menuToShow, PlayerEntity playerEntity)
        {
            var menu = GetMenu(menuToShow);
            if (menu == null)
            {
                Debug.LogWarning($"Could not find menu {menuToShow} in scene");
                return;
            }
            
            menu.ShowFor(playerEntity);
        }

        public void HideMenu(Menu menuToHide)
        {
            var menu = GetMenu(menuToHide);
            if (menu == null)
            {
                Debug.LogWarning($"Could not find menu {menuToHide} in scene");
                return;
            }
            
            menu.Hide();
        }

        private IMenu GetMenu(Menu menu)
        {
            switch (menu)
            {
                case Menu.Customization:
                    return customizationUI;
                default:
                    throw new ArgumentOutOfRangeException(nameof(menu), menu, null);
            }
        }
    }
}