using Canvases.Components;
using Sirenix.OdinInspector;
using Systems.Sound;
using UnityEngine;

namespace Canvases.Menu.Options
{
    public class OptionsUI : AbstractMenu
    {
        [SerializeField, Required] private SliderUIComponent masterVolumeSlider;
        [SerializeField, Required] private SliderUIComponent musicVolumeSlider;
        [SerializeField, Required] private SliderUIComponent effectsVolumeSlider;
        [SerializeField, Required] private ButtonUIComponent controlsButton;
        [SerializeField, Required] private ButtonUIComponent applyButton;
        
        protected override EntryDirection EnterDirection => EntryDirection.Down;
        protected override EntryDirection LeaveDirection => EntryDirection.Down;

        protected override bool ShowImplementation()
        {
            if (!base.ShowImplementation())
                return false;
            
            Init();
            return true;
        }

        private void Init()
        {
            masterVolumeSlider.Value = SoundSystem.Instance.GetMasterVolume();
            musicVolumeSlider.Value = SoundSystem.Instance.GetMusicVolume();
            effectsVolumeSlider.Value = SoundSystem.Instance.GetSoundEffectsVolume();

            if (MenuManager.HasInstance && !MenuManager.Instance.HasMenu(MenuManager.Menu.Controls))
            {
                controlsButton.Enabled = false;
            }
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            masterVolumeSlider.OnValueChanged += OnMasterVolumeChanged;
            musicVolumeSlider.OnValueChanged += OnMusicVolumeChanged;
            effectsVolumeSlider.OnValueChanged += OnEffectsVolumeChanged;
            controlsButton.OnClick += OnControlsPressed;
            applyButton.OnClick += OnApplyPressed;
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            masterVolumeSlider.OnValueChanged -= OnMasterVolumeChanged;
            musicVolumeSlider.OnValueChanged -= OnMusicVolumeChanged;
            effectsVolumeSlider.OnValueChanged -= OnEffectsVolumeChanged;
            controlsButton.OnClick -= OnControlsPressed;
            applyButton.OnClick -= OnApplyPressed;
        }

        private void OnMasterVolumeChanged(float newValue)
        {
            SoundSystem.Instance.SetMasterVolume(newValue);
        }

        private void OnMusicVolumeChanged(float newValue)
        {
            SoundSystem.Instance.SetMusicVolume(newValue);
        }
        
        private void OnEffectsVolumeChanged(float newValue)
        {
            SoundSystem.Instance.SetSoundEffectsVolume(newValue);
        }

        private void OnControlsPressed()
        {
            if (MenuManager.HasInstance)
            {
                MenuManager.Instance.ShowMenu(MenuManager.Menu.Controls);
            }
        }

        private void OnApplyPressed()
        {
            Hide();
        }
        
    }
}