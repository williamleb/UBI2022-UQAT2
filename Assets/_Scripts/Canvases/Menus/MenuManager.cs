using System;
using Canvases.Menu.Customization;
using Units.Player;
using UnityEngine;
using Utilities.Singleton;

namespace Canvases.Menu
{
    public class MenuManager : Singleton<MenuManager>
    {
        public event Action<bool> InMenuStatusChanged; 

        public enum Menu { Customization } 

        private CustomizationUI customizationUI;

        private int numberOfOpenedMenus;

        public bool InMenu => numberOfOpenedMenus > 0;

        private void Start()
        {
            customizationUI = FindObjectOfType<CustomizationUI>();
            SubscribeToMenus();
        }

        protected override void OnDestroy()
        {
            UnSubscribeToMenus();
            base.OnDestroy();
        }

        private void SubscribeToMenus()
        {
            customizationUI.OnShow += IncrementNumberOfOpenedMenus;
            customizationUI.OnHide += DecrementNumberOfOpenedMenus;
        }

        private void UnSubscribeToMenus()
        {
            customizationUI.OnShow -= IncrementNumberOfOpenedMenus;
            customizationUI.OnHide -= DecrementNumberOfOpenedMenus;
        }

        private void IncrementNumberOfOpenedMenus()
        {
            numberOfOpenedMenus++;
            UpdateInMenuStatus();
        }

        private void DecrementNumberOfOpenedMenus()
        {
            numberOfOpenedMenus--;
            UpdateInMenuStatus();
        }

        private void UpdateInMenuStatus()
        {
            InMenuStatusChanged?.Invoke(InMenu);
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