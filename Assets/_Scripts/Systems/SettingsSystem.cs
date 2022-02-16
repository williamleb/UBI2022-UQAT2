using System.Linq;
using Scriptables;
using UnityEngine;
using Utilities.Singleton;

namespace Systems
{
    public class SettingsSystem : PersistentSingleton<SettingsSystem>
    {
        private const string SETTINGS_FOLDER_PATH = "Settings";

        public PlayerSettings PlayerSetting { get; private set; }

        protected override void Awake()
        {
            base.Awake();

            LoadGlobalSettings();
        }

        private void LoadGlobalSettings()
        {
            PlayerSettings[] playerSetting = Resources.LoadAll<PlayerSettings>(SETTINGS_FOLDER_PATH);

            Debug.Assert(playerSetting.Any(), $"An object of type {nameof(PlayerSettings)} should be in the folder {SETTINGS_FOLDER_PATH}");
            if (playerSetting.Length > 1)
                Debug.LogWarning($"More than one object of type {nameof(PlayerSettings)} was found in the folder {SETTINGS_FOLDER_PATH}. Taking the first one.");

            PlayerSetting = playerSetting.First();
        }
    }
}