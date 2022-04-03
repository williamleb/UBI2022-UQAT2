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

        [Header("Animation")]
        [SerializeField] private bool isChangeValueAnimated = false;
        [SerializeField] [Range(0f, 1f)] private float animationSpeed = 0.05f;

        private EventTrigger.Entry selectEventTriggerEntry;
        private CanvasGroup parentCanvasGroup;
 
        private float targetValue;
        private float currentValue;

        private bool isLastValueChangeNegative;
        private Color initalColor = Color.black;
        public Color ValueIncreaseColor = Color.green;
        public Color ValueDecreaseColor = Color.red;

        public float Value
        {
            get => currentValue;
            set {
                if (isChangeValueAnimated)
                {
                    targetValue = value;

                    if (targetValue < currentValue)
                        isLastValueChangeNegative = true;
                    else
                        isLastValueChangeNegative = false;
                }
                else
                {
                    currentValue = value;
                }            
            }
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
                    initalColor = value;
                    fillImage.color = initalColor;
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

        private void Update()
        {
            if (isChangeValueAnimated)
            {
                if (!isLastValueChangeNegative)
                {
                    if (currentValue < targetValue)
                    {
                        currentValue += animationSpeed * Time.deltaTime;
                        slider.value = currentValue;
                        fillImage.color = ValueIncreaseColor;
                    }
                    else
                    {
                        currentValue = targetValue;
                        slider.value = currentValue;
                        fillImage.color = initalColor;
                    }
                }
                else {

                    if (currentValue > targetValue)
                    {
                        currentValue -= animationSpeed * Time.deltaTime;
                        slider.value = currentValue;
                        fillImage.color = ValueDecreaseColor;
                    }
                    else
                    {
                        currentValue = targetValue;
                        slider.value = currentValue;
                        fillImage.color = initalColor;
                    }
                }
            }
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