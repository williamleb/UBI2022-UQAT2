using System;
using Sirenix.OdinInspector;
using Systems.Sound;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Utilities.Extensions;

namespace Canvases.Components
{
    // ReSharper disable once InconsistentNaming Reason: UI should be capitalized
    public class SliderUIComponent : UIComponentBase
    {
        public event Action<float> OnValueChanged;
        public event Action OnSelected;
        
        [Header("Association")] 
        [SerializeField] [Required] private Slider slider;
        [SerializeField] private Image backgroundImage;
        [SerializeField] private Image fillImage;
        [SerializeField] private Image frame;
        
        private EventTrigger.Entry selectEventTriggerEntry;
        private CanvasGroup parentCanvasGroup;

        public float Value
        {
            get => slider.value;
            set => slider.value = value;
        }

        public float MaxValue
        {
            get => slider.maxValue;
            set => slider.maxValue = value;
        }
        
        public Color BackgroundColor
        {
            set
            {
                if (backgroundImage != null)
                {
                    backgroundImage.color = value;
                }
            }
        }
        
        public Color FillColor
        {
            set
            {
                if (fillImage != null)
                {
                    fillImage.color = value;
                }
            }
        }
        public Color FrameColor
        {
            set
            {
                if (frame != null)
                {
                    frame.color = value;
                }
            }
        }
        
        private void Awake()
        {
            AddSelectEventTrigger();
            parentCanvasGroup = gameObject.GetComponentInParents<CanvasGroup>();
        }

        private void AddSelectEventTrigger()
        {
            var eventTrigger = slider.gameObject.GetOrAddComponent<EventTrigger>();
            selectEventTriggerEntry = new EventTrigger.Entry
            {
                eventID = EventTriggerType.Select
            };
            selectEventTriggerEntry.callback.AddListener( OnSliderSelected );
            eventTrigger.triggers.Add(selectEventTriggerEntry);
        }

        private void Start()
        {
            Debug.Assert(slider, $"A {nameof(slider)} must be assigned to a {nameof(SliderUIComponent)}");
            slider.onValueChanged.AddListener(OnSliderChanged);
        }

        private void OnDestroy()
        {
            slider.onValueChanged.RemoveListener(OnSliderChanged);
            
            if (selectEventTriggerEntry != null)
                selectEventTriggerEntry.callback.RemoveListener(OnSliderSelected);
        }
        
        private void OnSliderChanged(float value)
        {
            OnValueChanged?.Invoke(value);
        }
        
        private void OnSliderSelected(BaseEventData data)
        {
            if (!slider.interactable || parentCanvasGroup != null && !parentCanvasGroup.interactable)
                return;
            
            SoundSystem.Instance.PlayMenuElementSelectSound();
            OnSelected?.Invoke();
        }

        private void OnValidate()
        {
            if (!slider)
                slider = GetComponent<Slider>();
        }
    }
}