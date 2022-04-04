using Systems.Settings;
using UnityEngine;

namespace Units.Customization
{
    [RequireComponent(typeof(CustomizationBase))]
    public class OutlineUpdater : MonoBehaviour
    {
        [SerializeField] private Outline.Mode outlineMode = Outline.Mode.OutlineAndSilhouette;
        [SerializeField] private float outlineWidth = 1.5f;
        [SerializeField] private bool matchColorWithClothes = true;
        
        private CustomizationBase customization;

        private void Awake()
        {
            customization = GetComponent<CustomizationBase>();
        }

        private void Start()
        {
            customization.OnHeadChangedEvent += RefreshOutline;
            customization.OnEyesChangedEvent += RefreshOutline;
            
            if (matchColorWithClothes) 
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
            foreach (var outlineCustomizer in GetComponentsInChildren<OutlineCustomizer>())
            {
                outlineCustomizer.EnableOutline();
                outlineCustomizer.OutlineMode = outlineMode;
                outlineCustomizer.OutlineWidth = outlineWidth;
            }
            
            if (matchColorWithClothes) 
                ChangeOutlineColor(customization.ClothesColor);
        }

        private void ChangeOutlineColor(int color)
        {
            var newColor = SettingsSystem.CustomizationSettings.GetColor(color);
            foreach (var outlineCustomizer in GetComponentsInChildren<OutlineCustomizer>())
            {
                outlineCustomizer.ChangeColor(newColor);
            }
        }
    }
}