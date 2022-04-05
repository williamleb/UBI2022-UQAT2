using Sirenix.OdinInspector;
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
        [SerializeField, HideIf(nameof(matchColorWithClothes))] private Color outlineColor;
        
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
            else
                ChangeOutlineColor(outlineColor);
        }

        private void ChangeOutlineColor(int color)
        {
            var newColor = SettingsSystem.CustomizationSettings.GetColor(color);
            ChangeOutlineColor(newColor);
        }

        private void ChangeOutlineColor(Color color)
        {
            foreach (var outlineCustomizer in GetComponentsInChildren<OutlineCustomizer>())
            {
                outlineCustomizer.ChangeColor(color);
            }
        }
    }
}