using System.Linq;
using Scriptables;
using UnityEngine;
using Utilities.Singleton;

namespace Systems
{
    public class SettingsSystem : PersistentSingleton<SettingsSystem>
    {
        private const string SettingsFolderPath = "Globals";

        private GlobalSettings settings;

        public GlobalSettings Settings => settings;
        
        protected override void Awake()
        {
            base.Awake();

            LoadGlobalSettings();
        }

        private void LoadGlobalSettings()
        {
            var globalSettings = Resources.LoadAll<GlobalSettings>(SettingsFolderPath);

            Debug.Assert(globalSettings.Any(), $"An object of type {nameof(GlobalSettings)} should be in the folder {SettingsFolderPath}");
            if (globalSettings.Length > 1)
                Debug.LogWarning($"More than one object of type {nameof(GlobalSettings)} was found in the folder {SettingsFolderPath}. Taking the first one.");

            settings = globalSettings.First();
        }
    }
}