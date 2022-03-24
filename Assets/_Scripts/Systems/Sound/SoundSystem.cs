using System.Linq;
using Sirenix.OdinInspector;
using Units.AI;
using Units.Player;
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

        public void PlayFootstepSound(PlayerEntity player) => wwiseObjects.FootstepEvent.Post(player.gameObject);
        public void PlayFootstepSound(AIEntity ai) => wwiseObjects.FootstepEvent.Post(ai.gameObject);
        public void PlayFumbleSound(PlayerEntity player) => wwiseObjects.FumbleEvent.Post(player.gameObject);
        public void PlayFumbleSound(AIEntity ai) => wwiseObjects.FumbleEvent.Post(ai.gameObject);
        public void PlayDashSound(PlayerEntity player) => wwiseObjects.DashEvent.Post(player.gameObject);
        public void PlayHandInHomeworkSound(PlayerEntity player) => wwiseObjects.HandInHomeworkEvent.Post(player.gameObject);
        public void PlayHandInHomeworkSound(AIEntity ai) => wwiseObjects.HandInHomeworkEvent.Post(ai.gameObject);
        public void PlayPickUpHomeworkSound(PlayerEntity player) => wwiseObjects.PickUpHomeworkEvent.Post(player.gameObject);
        public void PlayPickUpHomeworkSound(AIEntity ai) => wwiseObjects.PickUpHomeworkEvent.Post(ai.gameObject);
        public void PlayAimHoldSound(PlayerEntity player) => wwiseObjects.AimHoldEvent.Post(player.gameObject);
        public void StopAimHoldSound(PlayerEntity player) => wwiseObjects.AimHoldEvent.Stop(player.gameObject);
        public void PlayAimReleaseSound(PlayerEntity player) => wwiseObjects.AimReleaseEvent.Post(player.gameObject);


        public void SetMasterVolume(float volume) => wwiseObjects.MasterVolumeParameter.SetGlobalValue(volume * 100f);
        public void SetMusicVolume(float volume) => wwiseObjects.MusicVolumeParameter.SetGlobalValue(volume * 100f);
        public void SetSoundEffectsVolume(float volume) => wwiseObjects.SoundEffectsVolumeParameter.SetGlobalValue(volume * 100f);
        public void SetAimCharge(PlayerEntity player, float charge) => wwiseObjects.AimChargeParameter.SetValue(player.gameObject, charge * 100f);

        
#if UNITY_EDITOR
        private bool showDebugMenu;

        [Button("ToggleDebugMenu")]
        private void ToggleDebugMenu()
        {
            showDebugMenu = !showDebugMenu;
        }
        
        private void OnGUI()
        {
            if (showDebugMenu)
            {
                if (GUI.Button(new Rect(0, 0, 200, 40), "Footsteps"))
                {
                    wwiseObjects.FootstepEvent.Post(gameObject);
                }

                if (GUI.Button(new Rect(0, 40, 200, 40), "Fumble"))
                {
                    wwiseObjects.FumbleEvent.Post(gameObject);
                }

                if (GUI.Button(new Rect(0, 80, 200, 40), "HandInHomework"))
                {
                    wwiseObjects.FumbleEvent.Post(gameObject);
                }

                if (GUI.Button(new Rect(0, 120, 200, 40), "PickUpHomework"))
                {
                    wwiseObjects.PickUpHomeworkEvent.Post(gameObject);
                }
            }
        }
#endif
    }
}