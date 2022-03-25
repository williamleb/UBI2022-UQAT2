using System;
using System.Collections.Generic;
using Canvases.Menu.Customization;
using Canvases.Menu.Main;
using Canvases.Menu.Modal;
using JetBrains.Annotations;
using Units.Player;
using UnityEngine;
using Utilities.Singleton;

namespace Canvases.Menu
{
    public class MenuManager : Singleton<MenuManager>
    {
        public event Action<bool> InMenuStatusChanged; 

        public enum Menu { Customization, Main }

        private ModalUI modal;
        private int numberOfOpenedMenus;

        private Dictionary<Menu, AbstractMenu> menus = new Dictionary<Menu, AbstractMenu>();

        public bool InMenu => numberOfOpenedMenus > 0;

        private void Start()
        {
            InitializeMenus();
            SubscribeToMenus();
        }

        private void InitializeMenus()
        {
            modal = FindObjectOfType<ModalUI>();
            var customizationUI = FindObjectOfType<CustomizationUI>();
            var mainUI = FindObjectOfType<MainUI>();

            menus.Add(Menu.Customization, customizationUI);
            menus.Add(Menu.Main, mainUI);
        }

        protected override void OnDestroy()
        {
            UnSubscribeToMenus();
            base.OnDestroy();
        }

        private void SubscribeToMenus()
        {
            foreach (var menu in menus.Values)
            {
                if (!menu)
                    continue;
                
                menu.OnShow += IncrementNumberOfOpenedMenus;
                menu.OnHide += DecrementNumberOfOpenedMenus;
            }
        }

        private void UnSubscribeToMenus()
        {
            foreach (var menu in menus.Values)
            {
                if (!menu)
                    continue;
                
                menu.OnShow -= IncrementNumberOfOpenedMenus;
                menu.OnHide -= DecrementNumberOfOpenedMenus;
            }
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

        public void ShowModal(string text, string header, float seconds = 5f)
        {
            if (!modal)
                return;
            
            modal.Show(text, header, seconds);
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
        
        public void ShowMenu(Menu menuToShow)
        {
            var menu = GetMenu(menuToShow);
            if (menu == null)
            {
                Debug.LogWarning($"Could not find menu {menuToShow} in scene");
                return;
            }
            
            menu.Show();
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

        [CanBeNull]
        private IMenu GetMenu(Menu menu)
        {
            if (menus.ContainsKey(menu))
                return menus[menu];
            
            return null;
        }
    }
}