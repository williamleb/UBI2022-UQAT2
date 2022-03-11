using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Utilities.Singleton;

namespace Systems.Settings
{
    public class SettingsSystem : PersistentSingleton<SettingsSystem>
    {
        private const string SETTINGS_FOLDER_PATH = "Settings";

        public static PlayerSettings[] PlayerSettings => Instance.PlayerSetting;
        public static AISettings AISettings => Instance.AISetting;
        public static MatchmakingSettings MatchmakingSettings => Instance.MatchmakingSetting;
        public static HomeworkSettings HomeworkSettings => Instance.HomeworkSetting;

        private PlayerSettings[] playerSettings;
        private AISettings aiSetting;
        private MatchmakingSettings matchmakingSetting;
        private HomeworkSettings homeworkSetting;

        public PlayerSettings[] PlayerSetting => playerSettings;
        public AISettings AISetting => aiSetting;
        public MatchmakingSettings MatchmakingSetting => matchmakingSetting;
        public HomeworkSettings HomeworkSetting => homeworkSetting;

        protected override void Awake()
        {
            base.Awake();

            LoadAllSettings(out playerSettings);
            LoadSettings(out aiSetting);
            LoadSettings(out homeworkSetting);
            LoadSettings(out matchmakingSetting);
        }

        private void LoadSettings<T>(out T memberToInitialize) where T : ScriptableObject
        {
            var loadedSettingsList = Resources.LoadAll<T>(SETTINGS_FOLDER_PATH);

            Debug.Assert(loadedSettingsList.Any(), $"An object of type {typeof(T).Name} should be in the folder {SETTINGS_FOLDER_PATH}");
            if (loadedSettingsList.Length > 1)
                Debug.LogWarning($"More than one object of type {typeof(T).Name} was found in the folder {SETTINGS_FOLDER_PATH}. Taking the first one.");

            memberToInitialize = loadedSettingsList.First();
        }
        
        private void LoadAllSettings<T>(out T[] memberToInitialize) where T : ScriptableObject
        {
            var loadedSettingsList = Resources.LoadAll<T>(SETTINGS_FOLDER_PATH);

            Debug.Assert(loadedSettingsList.Any(), $"An object of type {typeof(T).Name} should be in the folder {SETTINGS_FOLDER_PATH}");
            if (loadedSettingsList.Length > 1)
                Debug.LogWarning($"More than one object of type {typeof(T).Name} was found in the folder {SETTINGS_FOLDER_PATH}. Taking the first one.");

            memberToInitialize = loadedSettingsList;
        }
    }
}