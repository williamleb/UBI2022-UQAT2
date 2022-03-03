using System.Linq;
using Scriptables;
using UnityEngine;
using Utilities.Singleton;

namespace Systems
{
    public class SettingsSystem : PersistentSingleton<SettingsSystem>
    {
        private const string SETTINGS_FOLDER_PATH = "Settings";

        private PlayerSettings playerSettings;
        private AISettings aiSettings;
        private HomeworkSettings homeworkSettings;

        public PlayerSettings PlayerSetting => playerSettings;
        public AISettings AISettings => aiSettings;
        public HomeworkSettings HomeworkSettings => homeworkSettings;

        protected override void Awake()
        {
            base.Awake();

            LoadSettings(out playerSettings);
            LoadSettings(out aiSettings);
            LoadSettings(out homeworkSettings);
        }

        private void LoadSettings<T>(out T memberToInitialize) where T : ScriptableObject
        {
            var loadedSettingsList = Resources.LoadAll<T>(SETTINGS_FOLDER_PATH);

            Debug.Assert(loadedSettingsList.Any(), $"An object of type {typeof(T).Name} should be in the folder {SETTINGS_FOLDER_PATH}");
            if (loadedSettingsList.Length > 1)
                Debug.LogWarning($"More than one object of type {typeof(T).Name} was found in the folder {SETTINGS_FOLDER_PATH}. Taking the first one.");

            memberToInitialize = loadedSettingsList.First();
        }
    }
}