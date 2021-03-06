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
using Canvases.Menu.Rebind;
using JetBrains.Annotations;
using Units.Player;
using UnityEngine;
using Utilities.Singleton;

namespace Canvases.Menu
{
    public class MenuManager : Singleton<MenuManager>
    {
        public event Action<bool> InMenuStatusChanged;
        public event Action OnModalShow;
        public event Action OnModalHide;

        public enum Menu { Customization, Main, Host, Join, Options, Game, Controls, End }

        private ModalUI modal;
        private int numberOfOpenedMenus;
        private List<AbstractMenu> menusToReturnTo = new List<AbstractMenu>();
        private bool isOpened = true;

        private readonly Dictionary<Menu, AbstractMenu> menus = new Dictionary<Menu, AbstractMenu>();

        public bool InMenu => numberOfOpenedMenus > 0;
        public bool IsOpened => isOpened;

        public void Open()
        {
            isOpened = true;
            foreach (var menu in menus.Values)
            {
                menu.OnMenuManagerOpened();
            }
        }

        public void Close()
        {
            isOpened = false;
            foreach (var menu in menus.Values)
            {
                menu.OnMenuManagerClosed();
            }
        }

        public bool IsInTransition(Menu menu)
        {
            if (!menus.ContainsKey(menu))
                return false;

            return menus[menu].IsInTransition;
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

            if(modal != null)
            {
                modal.OnHide += ModalHide;
                modal.OnShow += ModalShow;
            }            

            TryAddToMenus<CustomizationUI>(Menu.Customization);
            TryAddToMenus<MainUI>(Menu.Main);
            TryAddToMenus<HostJoinUI>(Menu.Host, menu => menu.Host);
            TryAddToMenus<HostJoinUI>(Menu.Join, menu => !menu.Host);
            TryAddToMenus<OptionsUI>(Menu.Options);
            TryAddToMenus<GameUI>(Menu.Game);
            TryAddToMenus<RebindUI>(Menu.Controls);
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

        public void UnfocusEverything()
        {
            foreach (var menu in menus)
            {
                if (menu.Value) menu.Value.Unfocus();
            }
            menusToReturnTo.Clear();
        }
        
        public void PushMenuToReturnTo(AbstractMenu menu)
        {
            menusToReturnTo.Add(menu);
        }

        public void RemoveButtonToReturnTo(AbstractMenu menu)
        {
            menusToReturnTo.Remove(menu);
        }

        public void ReturnToMenu()
        {
            if (!menusToReturnTo.Any())
                return;
            
            menusToReturnTo.Last().Focus();
        }

        public bool HasMenu(Menu menuType)
        {
            return menus.ContainsKey(menuType);
        }

        protected override void OnDestroy()
        {
            if(modal != null)
            {
                modal.OnHide -= ModalHide;
                modal.OnShow -= ModalShow;
            }

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

        public void ModalShow()
        {
            OnModalShow?.Invoke();
        }

        public void ModalHide()
        {
            OnModalHide?.Invoke();
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