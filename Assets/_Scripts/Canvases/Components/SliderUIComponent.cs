using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Canvases.Components
{
    public class SliderUIComponent : UIComponentBase
    {
        [SerializeField] private Slider slider;

        public void SetValue(float value, float max)
        {
            slider.maxValue = max;
            slider.value = value;
        }

        public void SetValue(float value) => slider.value = value;

        public float GetValue() => slider.value;

        public void OnValueChanged(UnityAction<float> callBack) => slider.onValueChanged.AddListener(callBack);

        private void OnDestroy() => slider.onValueChanged?.RemoveAllListeners();
        
        private void OnValidate() => slider = GetComponent<Slider>();
    }
}