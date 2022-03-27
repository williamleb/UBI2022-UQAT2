using System;
using System.Collections.Generic;
using System.Linq;
using Canvases.Components;
using Canvases.Matchmaking;
using Canvases.Menu.Customization;
using Canvases.Menu.Game;
using Canvases.Menu.Main;
using Canvases.Menu.Modal;
using Canvases.Menu.Options;
using JetBrains.Annotations;
using Units.Player;
using UnityEngine;
using Utilities.Singleton;

namespace Canvases.Menu
{
    public class MenuManager : Singleton<MenuManager>
    {
        public event Action<bool> InMenuStatusChanged; 

        public enum Menu { Customization, Main, Host, Join, Options, Game, Controls, End }

        private ModalUI modal;
        private int numberOfOpenedMenus;
        private List<ButtonUIComponent> buttonsToReturnTo = new List<ButtonUIComponent>();

        private readonly Dictionary<Menu, AbstractMenu> menus = new Dictionary<Menu, AbstractMenu>();

        public bool InMenu => numberOfOpenedMenus > 0;

        public void Open()
        {
            foreach (var menu in menus.Values)
            {
                menu.OnMenuManagerOpened();
            }
        }

        public void Close()
        {
            foreach (var menu in menus.Values)
            {
                menu.OnMenuManagerClosed();
            }
        }

        private void Start()
        {
            InitializeMenus();
            SubscribeToMenus();
            Open();
        }

        private void InitializeMenus()
        {
            modal = FindObjectOfType<ModalUI>();
            TryAddToMenus<CustomizationUI>(Menu.Customization);
            TryAddToMenus<MainUI>(Menu.Main);
            TryAddToMenus<HostJoinUI>(Menu.Host, menu => menu.Host);
            TryAddToMenus<HostJoinUI>(Menu.Join, menu => !menu.Host);
            TryAddToMenus<OptionsUI>(Menu.Options);
            TryAddToMenus<GameUI>(Menu.Game);
        }

        private void TryAddToMenus<T>(Menu menuType, Func<T, bool> validator = null) where T : AbstractMenu
        {
            var menusOfType = FindObjectsOfType<T>();

            foreach (var menu in menusOfType)
            {
                if (menu.gameObject.scene != gameObject.scene)
                    continue;
                
                if (validator != null && !validator.Invoke(menu))
                    continue;
                
                menus.Add(menuType, menu);
            }
        }
        
        public void PushButtonToReturnTo(ButtonUIComponent button)
        {
            buttonsToReturnTo.Add(button);
        }

        public void RemoveButtonToReturnTo(ButtonUIComponent button)
        {
            buttonsToReturnTo.Remove(button);
        }

        public void ReturnToButton()
        {
            if (!buttonsToReturnTo.Any())
                return;
            
            buttonsToReturnTo.Last().Select();
        }

        public bool HasMenu(Menu menuType)
        {
            return menus.ContainsKey(menuType);
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

        public bool ShowModal(string text, string header, float seconds = 5f)
        {
            if (!modal)
                return false;
            
            return modal.Show(text, header, seconds);
        }

        public bool ShowMenuForPlayer(Menu menuToShow, PlayerEntity playerEntity) 
        {
            var menu = GetMenu(menuToShow);
            if (menu == null)
            {
                Debug.LogWarning($"Could not find menu {menuToShow} in scene");
                return false;
            }
            
            return menu.ShowFor(playerEntity);
        }
        
        public bool ShowMenu(Menu menuToShow)
        {
            var menu = GetMenu(menuToShow);
            if (menu == null)
            {
                Debug.LogWarning($"Could not find menu {menuToShow} in scene");
                return false;
            }
            
            return menu.Show();
        }

        public bool HideMenu(Menu menuToHide)
        {
            var menu = GetMenu(menuToHide);
            if (menu == null)
            {
                Debug.LogWarning($"Could not find menu {menuToHide} in scene");
                return false;
            }
            
            return menu.Hide();
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