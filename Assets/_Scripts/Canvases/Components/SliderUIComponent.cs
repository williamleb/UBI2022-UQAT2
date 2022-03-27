using System;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

namespace Canvases.Components
{
    // ReSharper disable once InconsistentNaming Reason: UI should be capitalized
    public class SliderUIComponent : UIComponentBase
    {
        public event Action<float> OnValueChanged;
        
        [Header("Association")] 
        [SerializeField] [Required] private Slider slider;
        [SerializeField] private Image backgroundImage;
        [SerializeField] private Image fillImage;

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

        private void Start()
        {
            Debug.Assert(slider, $"A {nameof(slider)} must be assigned to a {nameof(SliderUIComponent)}");
            slider.onValueChanged.AddListener(OnSliderChanged);
        }

        private void OnDestroy()
        {
            slider.onValueChanged.RemoveListener(OnSliderChanged);
        }
        
        private void OnSliderChanged(float value)
        {
            OnValueChanged?.Invoke(value);
        }

        private void OnValidate()
        {
            if (!slider)
                slider = GetComponent<Slider>();
        }
    }
}