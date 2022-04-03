using Systems.Settings;
using UnityEngine;

namespace Units.Customization
{
    [RequireComponent(typeof(CustomizationBase))]
    public class OutlineUpdater : MonoBehaviour
    {
        private CustomizationBase customization;

        private void Awake()
        {
            customization = GetComponent<CustomizationBase>();
        }

        private void Start()
        {
            customization.OnHeadChangedEvent += RefreshOutline;
            customization.OnEyesChangedEvent += RefreshOutline;
            customization.OnClothesColorChangedEvent += ChangeOutlineColor;
            
            RefreshOutline();
        }

        private void OnDestroy()
        {
            customization.OnHeadChangedEvent -= RefreshOutline;
            customization.OnEyesChangedEvent -= RefreshOutline;
            customization.OnClothesColorChangedEvent -= ChangeOutlineColor;
        }

        private void RefreshOutline(int _ = 0)
        {
            ChangeOutlineColor(customization.ClothesColor);
        }

        private void ChangeOutlineColor(int color)
        {
            var newColor = SettingsSystem.CustomizationSettings.GetColor(color);
            foreach (var outlineCustomizer in GetComponentsInChildren<OutlineCustomizer>())
            {
                // outlineCustomizer.EnableOutline();
                // outlineCustomizer.ChangeColor(newColor);
            }
        }
    }
}