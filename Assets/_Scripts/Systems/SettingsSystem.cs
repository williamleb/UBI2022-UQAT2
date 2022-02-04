using System.Linq;
using Scriptables;
using UnityEngine;
using Utilities.Singleton;

namespace Systems
{
    public class SettingsSystem : PersistentSingleton<SettingsSystem>
    {
        private const string SettingsFolderPath = "Settings";

        private PlayerSettings playerSettings;
        public PlayerSettings PlayerSetting => playerSettings;
        
        protected override void Awake()
        {
            base.Awake();

            LoadGlobalSettings();
        }

        private void LoadGlobalSettings()
        {
            var playerSetting = Resources.LoadAll<PlayerSettings>(SettingsFolderPath);

            Debug.Assert(playerSetting.Any(), $"An object of type {nameof(PlayerSettings)} should be in the folder {SettingsFolderPath}");
            if (playerSetting.Length > 1)
                Debug.LogWarning($"More than one object of type {nameof(PlayerSettings)} was found in the folder {SettingsFolderPath}. Taking the first one.");

            playerSettings = playerSetting.First();
        }
    }
}