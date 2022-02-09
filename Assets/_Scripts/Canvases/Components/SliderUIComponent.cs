using System;
using UnityEngine;
using UnityEngine.UI;

namespace Canvases.Components
{
    // ReSharper disable once InconsistentNaming Reason: UI should be capitalized
    public class SliderUIComponent : UIComponentBase
    {
        public event Action<float> OnValueChanged;
        
        [Header("Association")]
        [SerializeField] private Slider slider;

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
    }
}