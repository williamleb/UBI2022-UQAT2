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
        public static GameSettings GameSettings => Instance.GameSetting;
        public static CustomisationSettings CustomisationSettings => Instance.CustomisationSetting;

        private PlayerSettings[] playerSettings;
        private AISettings aiSetting;
        private MatchmakingSettings matchmakingSetting;
        private HomeworkSettings homeworkSetting;
        private GameSettings gameSetting;
        private CustomisationSettings customisationSettings;

        private PlayerSettings[] PlayerSetting => playerSettings;
        private AISettings AISetting => aiSetting;
        private MatchmakingSettings MatchmakingSetting => matchmakingSetting;
        private HomeworkSettings HomeworkSetting => homeworkSetting;
        private GameSettings GameSetting => gameSetting;
        private CustomisationSettings CustomisationSetting => customisationSettings;

        protected override void Awake()
        {
            base.Awake();

            LoadAllSettings(out playerSettings);
            LoadSettings(out aiSetting);
            LoadSettings(out homeworkSetting);
            LoadSettings(out matchmakingSetting);
            LoadSettings(out gameSetting);
            LoadSettings(out customisationSettings);
        }

        private void LoadSettings<T>(out T memberToInitialize) where T : ScriptableObject
        {
            var loadedSettingsList = Resources.LoadAll<T>(SETTINGS_FOLDER_PATH);

            Debug.Assert(loadedSettingsList.Any(),
                $"An object of type {typeof(T).Name} should be in the folder {SETTINGS_FOLDER_PATH}");
            if (loadedSettingsList.Length > 1)
                Debug.LogWarning(
                    $"More than one object of type {typeof(T).Name} was found in the folder {SETTINGS_FOLDER_PATH}. Taking the first one.");

            memberToInitialize = loadedSettingsList.First();
        }

        private void LoadAllSettings<T>(out T[] memberToInitialize) where T : ScriptableObject
        {
            var loadedSettingsList = Resources.LoadAll<T>(SETTINGS_FOLDER_PATH);

            Debug.Assert(loadedSettingsList.Any(),
                $"An object of type {typeof(T).Name} should be in the folder {SETTINGS_FOLDER_PATH}");

            memberToInitialize = loadedSettingsList;
        }
    }
}