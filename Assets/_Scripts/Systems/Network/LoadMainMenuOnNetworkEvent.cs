using System.Collections.Generic;
using System.Linq;
using Canvases.Menu;
using Fusion;
using Systems.Level;
using Systems.Settings;
using UnityEngine;

namespace Systems.Network
{
    public class LoadMainMenuOnNetworkEvent : MonoBehaviour
    {
        [SerializeField] private List<ShutdownReason> shutdownReasonsFilter = new List<ShutdownReason>();

        private NetworkSettings settings;

        private void Start()
        {
            settings = SettingsSystem.NetworkSettings;
            NetworkSystem.Instance.OnShutdownEvent += OnServerShutdown;
        }

        private void OnDestroy()
        {
            if (NetworkSystem.HasInstance)
                NetworkSystem.Instance.OnShutdownEvent -= OnServerShutdown;
        }

        private void OnServerShutdown(NetworkRunner runner, ShutdownReason shutdownReason)
        {
            if (shutdownReasonsFilter.Any() && !shutdownReasonsFilter.Contains(shutdownReason))
                return;

            if (MenuManager.HasInstance)
            {
                MenuManager.Instance.OnModalHide += OnConnectionLostModalHide;
                MenuManager.Instance.ShowModal(settings.HostConnectionLostMessage, settings.HostConnectionLostHeader);
            }
        }

        private void OnConnectionLostModalHide()
        {
            MenuManager.Instance.OnModalHide -= OnConnectionLostModalHide;
            LevelSystem.Instance.LoadMainMenu();
            NetworkSystem.Instance.Disconnect();
        }
    }
}