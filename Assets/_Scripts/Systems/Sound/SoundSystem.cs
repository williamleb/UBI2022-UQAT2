using System.Linq;
using UnityEngine;
using Utilities.Singleton;

 namespace Systems.Sound
{
    public class SoundSystem : PersistentSingleton<SoundSystem>
    {
        private const string SOUNDS_FOLDER_PATH = "Wwise";

        private WwiseObjects wwiseObjects;
        
        protected override void Awake()
        {
            base.Awake();

            LoadSoundEvents();
            LoadBank();
        }

        private void LoadSoundEvents()
        {
            var soundEvents = Resources.LoadAll<WwiseObjects>(SOUNDS_FOLDER_PATH);

            Debug.Assert(soundEvents.Any(), $"An object of type {nameof(WwiseObjects)} should be in the folder {SOUNDS_FOLDER_PATH}");
            if (soundEvents.Length > 1)
                Debug.LogWarning($"More than one object of type {nameof(WwiseObjects)} was found in the folder {SOUNDS_FOLDER_PATH}. Taking the first one.");

            wwiseObjects = soundEvents.First();
        }
        
        private void LoadBank()
        {
            Instantiate(wwiseObjects.SoundBankPrefab, transform);
        }

        public void PlayBababooeySound() => wwiseObjects.BababooeyEvent.Post(gameObject);

    }
}