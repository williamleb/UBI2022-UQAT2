using System;
using System.Linq;
using Units.Player;
using UnityEngine;
using Utilities.Singleton;

namespace Systems.Settings
{
    public class SettingsSystem : PersistentSingleton<SettingsSystem>
    {
        private const string SETTINGS_FOLDER_PATH = "Settings";

        public static PlayerSettings[] PlayerSettings => Instance.PlayerSetting;
        public static AISettings AISettings => Instance.AISetting;
        public static HomeworkSettings HomeworkSettings => Instance.HomeworkSetting;
        public static GameSettings GameSettings => Instance.GameSetting;
        public static TeamSettings TeamSettings => Instance.TeamSetting;
        public static CustomizationSettings CustomizationSettings => Instance.CustomizationSetting;
        public static HUDSettings HUDSettings => Instance.HUDSetting;
        public static NetworkSettings NetworkSettings => Instance.NetworkSetting;

        private PlayerSettings[] playerSettings;
        private AISettings aiSetting;
        private HomeworkSettings homeworkSetting;
        private GameSettings gameSetting;
        private TeamSettings teamSetting;
        private CustomizationSettings customizationSettings;
        private NetworkSettings networkSetting;

        private HUDSettings hudSetting;

        private PlayerSettings[] PlayerSetting => playerSettings;
        private AISettings AISetting => aiSetting;
        private HomeworkSettings HomeworkSetting => homeworkSetting;
        private GameSettings GameSetting => gameSetting;
        private TeamSettings TeamSetting => teamSetting;
        private CustomizationSettings CustomizationSetting => customizationSettings;
        private HUDSettings HUDSetting => hudSetting;
        private NetworkSettings NetworkSetting => networkSetting;

        public PlayerSettings GetPlayerSettings(Archetype archetype)
        {
            foreach (var settings in playerSettings)
            {
                if (settings.PlayerArchetype == archetype)
                    return settings;
            }

            throw new IndexOutOfRangeException();
        }

        protected override void Awake()
        {
            base.Awake();

            LoadAllSettings(out playerSettings);
            LoadSettings(out networkSetting);
            LoadSettings(out aiSetting);
            LoadSettings(out homeworkSetting);
            LoadSettings(out gameSetting);
            LoadSettings(out teamSetting);
            LoadSettings(out customizationSettings);
            LoadSettings(out hudSetting);
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