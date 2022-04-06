using System;
using System.Linq;
using Ingredients.Homework;
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
        private const string MASTER_VOLUME_SAVE_KEY = "master_volume";
        private const string MUSIC_VOLUME_SAVE_KEY = "music_volume";
        private const string SOUND_EFFECTS_VOLUME_SAVE_KEY = "sfx_volume";

        private WwiseObjects wwiseObjects;

        private bool isMusicPlaying;
        
        protected override void Awake()
        {
            base.Awake();

            LoadSoundEvents();
            LoadBank();
            LoadAllVolumes();
        }

        private void LoadSoundEvents()
        {
            var soundEvents = Resources.LoadAll<WwiseObjects>(SOUNDS_FOLDER_PATH);

            Debug.Assert(soundEvents.Any(), $"An object of type {nameof(WwiseObjects)} should be in the folder {SOUNDS_FOLDER_PATH}");
            if (soundEvents.Length > 1)
                Debug.LogWarning($"More than one object of type {nameof(WwiseObjects)} was found in the folder {SOUNDS_FOLDER_PATH}. Taking the first one.");

            wwiseObjects = soundEvents.First();
        }
        
        private void LoadBank() => Instantiate(wwiseObjects.SoundBankPrefab, transform);
        
        public void PlayFootstepSound(PlayerEntity player) => wwiseObjects.FootstepEvent.Post(player.gameObject);
        public void PlayFootstepSound(AIEntity ai) => wwiseObjects.FootstepEvent.Post(ai.gameObject);
        public void PlayFumbleSound(PlayerEntity player) => wwiseObjects.FumbleEvent.Post(player.gameObject);
        public void PlayFumbleSound(AIEntity ai) => wwiseObjects.FumbleEvent.Post(ai.gameObject);
        public void PlayDashSound(PlayerEntity player) => wwiseObjects.DashEvent.Post(player.gameObject);
        public void StopDashSound(PlayerEntity player) => wwiseObjects.DashEvent.Stop(player.gameObject);
        public void PlayDashCollisionSound(PlayerEntity player) => wwiseObjects.DashCollisionEvent.Post(player.gameObject);
        public void PlayHandInHomeworkSound(PlayerEntity player) => wwiseObjects.HandInHomeworkEvent.Post(player.gameObject);
        public void PlayHandInHomeworkSound(AIEntity ai) => wwiseObjects.HandInHomeworkEvent.Post(ai.gameObject);
        public void PlayPickUpHomeworkSound(PlayerEntity player) => wwiseObjects.PickUpHomeworkEvent.Post(player.gameObject);
        public void PlayPickUpHomeworkSound(AIEntity ai) => wwiseObjects.PickUpHomeworkEvent.Post(ai.gameObject);
        public void PlayInteractWorldElementSound() => wwiseObjects.InteractWorldElementEvent.Post(gameObject);
        public void PlayCharacterSelectSound(Archetype archetype)
        {
            switch (archetype)
            {
                case Archetype.Base:
                    wwiseObjects.CharacterSelectBaseEvent.Post(gameObject);
                    break;
                case Archetype.Runner:
                    wwiseObjects.CharacterSelectRunnerEvent.Post(gameObject);
                    break;
                case Archetype.Dasher:
                    wwiseObjects.CharacterSelectDasherEvent.Post(gameObject);
                    break;
                case Archetype.Thrower:
                    wwiseObjects.CharacterSelectThrowerEvent.Post(gameObject);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(archetype), archetype, null);
            }
        }
        public void PlayAimHoldSound(PlayerEntity player)
        {
            StopAimHoldSound(player);
            wwiseObjects.AimHoldEvent.Post(player.gameObject);
        }
        public void StopAimHoldSound(PlayerEntity player) => wwiseObjects.AimHoldEvent.Stop(player.gameObject);
        public void PlayAimReleaseSound(PlayerEntity player) => wwiseObjects.AimReleaseEvent.Post(player.gameObject);
        public void PlayHomeworkFlyingSound(Homework homework) => wwiseObjects.HomeworkFlyingEvent.Post(homework.gameObject);
        public void PlayVictoryJingleSound() => wwiseObjects.VictoryJingleEvent.Post(gameObject);
        public void PlayDefeatJingleSound() => wwiseObjects.DefeatJingleEvent.Post(gameObject);
        public void PlayOneSound() => wwiseObjects.OneEvent.Post(gameObject);
        public void PlayTwoSound() => wwiseObjects.TwoEvent.Post(gameObject);
        public void PlayThreeSound() => wwiseObjects.ThreeEvent.Post(gameObject);
        public void PlayGoSound() => wwiseObjects.GoEvent.Post(gameObject);
        public void PlayJanitorCaughtAlertSound(AIEntity janitor) => wwiseObjects.JanitorCaughtAlertEvent.Post(janitor.gameObject);
        public void PlayMenuElementSelectSound() => wwiseObjects.MenuElementSelectEvent.Post(gameObject);
        public void PlayMenuElementForwardSound() => wwiseObjects.MenuElementForwardEvent.Post(gameObject);
        public void PlayMenuElementBackwardSound() => wwiseObjects.MenuElementBackwardEvent.Post(gameObject);
        public void PlayOvertimeStartSound() => wwiseObjects.OvertimeStartEvent.Post(gameObject);

        public float GetMasterVolume() => wwiseObjects.MasterVolumeParameter.GetGlobalValue() / 100f;
        public float GetMusicVolume() => wwiseObjects.MusicVolumeParameter.GetGlobalValue() / 100f;
        public float GetSoundEffectsVolume() => wwiseObjects.SoundEffectsVolumeParameter.GetGlobalValue() / 100f;
        public void SetMasterVolume(float volume)
        {
            wwiseObjects.MasterVolumeParameter.SetGlobalValue(volume * 100f);
            SaveMasterVolume();
        }
        public void SetMusicVolume(float volume) 
        {
            wwiseObjects.MusicVolumeParameter.SetGlobalValue(volume * 100f);
            SaveMusicVolume();
        }
        public void SetSoundEffectsVolume(float volume)
        {
            wwiseObjects.SoundEffectsVolumeParameter.SetGlobalValue(volume * 100f);
            SaveSoundEffectsVolume();
        }
        public void SetAimCharge(PlayerEntity player, float charge) => wwiseObjects.AimChargeParameter.SetValue(player.gameObject, charge * 100f);

        public void PlayMusic()
        {
            if (isMusicPlaying)
                return;

            wwiseObjects.MusicEvent.Post(gameObject);
            isMusicPlaying = true;
        }

        public void SetInMenu() => wwiseObjects.InMenuState.SetValue();
        public void SetInGame() => wwiseObjects.InGameState.SetValue();
        public void SetInScoreboard() => wwiseObjects.InScoreboardState.SetValue();
        public void SetInOvertime() => wwiseObjects.InOvertimeState.SetValue();
        public void SetHoldingHomework() => wwiseObjects.HoldingHomeworkState.SetValue();

        private void OnDestroy()
        {
            if (isMusicPlaying)
                wwiseObjects.MusicEvent.Stop(gameObject);
            isMusicPlaying = false;
        }
        
        private void SaveMasterVolume()
        { 
            PlayerPrefs.SetFloat(MASTER_VOLUME_SAVE_KEY, wwiseObjects.MasterVolumeParameter.GetGlobalValue());
        }

        private void SaveMusicVolume()
        {
            PlayerPrefs.SetFloat(MUSIC_VOLUME_SAVE_KEY, wwiseObjects.MusicVolumeParameter.GetGlobalValue());
        }

        private void SaveSoundEffectsVolume()
        {
            PlayerPrefs.SetFloat(SOUND_EFFECTS_VOLUME_SAVE_KEY, wwiseObjects.SoundEffectsVolumeParameter.GetGlobalValue());
        }

        private void LoadAllVolumes()
        {
            if (PlayerPrefs.HasKey(MASTER_VOLUME_SAVE_KEY))
                wwiseObjects.MasterVolumeParameter.SetGlobalValue(PlayerPrefs.GetFloat(MASTER_VOLUME_SAVE_KEY));
            if (PlayerPrefs.HasKey(MUSIC_VOLUME_SAVE_KEY))
                wwiseObjects.MusicVolumeParameter.SetGlobalValue(PlayerPrefs.GetFloat(MUSIC_VOLUME_SAVE_KEY));
            if (PlayerPrefs.HasKey(SOUND_EFFECTS_VOLUME_SAVE_KEY))
                wwiseObjects.SoundEffectsVolumeParameter.SetGlobalValue(PlayerPrefs.GetFloat(SOUND_EFFECTS_VOLUME_SAVE_KEY));
        }

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
                if (GUI.Button(new Rect(0, 0, 200, 40), "Mute"))
                {
                    SetMasterVolume(0f);
                }

                if (GUI.Button(new Rect(0, 40, 200, 40), "Unmute"))
                {
                    SetMasterVolume(0.75f);
                }
            }
        }
#endif
    }
}