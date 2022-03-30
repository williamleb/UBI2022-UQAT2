using System;
using Fusion;
using Sirenix.OdinInspector;
using Systems.Settings;
using Units.Player;
using UnityEngine;

namespace Units.Customization
{
    public class CustomizationBase : NetworkBehaviour
    {
        public Action<int> OnHeadChangedEvent;
        public Action<int> OnHairColorChangedEvent;
        public Action<int> OnEyesChangedEvent;
        public Action<int> OnSkinChangedEvent;
        public Action<Archetype> OnClothesChangedEvent;
        public Action<int> OnClothesColorChangedEvent;

        [SerializeField, Required] protected CustomizationPoint HeadCustomizationPoint;
        [SerializeField, Required] protected CustomizationPoint FaceCustomizationPoint;
        [SerializeField, Required] protected CustomizationPoint NoseCustomizationPoint;
        [SerializeField, Required] protected CustomizationPoint LeftEyeCustomizationPoint;
        [SerializeField, Required] protected CustomizationPoint RightEyeCustomizationPoint;
        [SerializeField, Required] protected CustomizationPoint LeftAltEyeCustomizationPoint;
        [SerializeField, Required] protected CustomizationPoint RightAltEyeCustomizationPoint;

        protected CustomizationSettings Settings;

        // ReSharper disable Unity.RedundantAttributeOnTarget
        [Networked(OnChanged = nameof(OnHeadChanged)), HideInInspector]
        public int Head { get; protected set; }

        [Networked(OnChanged = nameof(OnHairColorChanged)), HideInInspector]
        public int HairColor { get; protected set; }

        [Networked(OnChanged = nameof(OnEyesChanged)), HideInInspector]
        public int Eyes { get; protected set; }

        [Networked(OnChanged = nameof(OnSkinChanged)), HideInInspector]
        public int Skin { get; protected set; }

        [Networked(OnChanged = nameof(OnClothesChanged)), HideInInspector]
        public Archetype Clothes { get; protected set; }

        [Networked(OnChanged = nameof(OnClothesColorChanged)), HideInInspector]
        public int ClothesColor { get; protected set; }
        // ReSharper restore Unity.RedundantAttributeOnTarget

        protected void UpdateAll()
        {
            UpdateHead();
            UpdateHairColor();
            UpdateEyes();
            UpdateSkin();
            UpdateClothes();
            UpdateClothesColor();
        }

        private void UpdateHead()
        {
            HeadCustomizationPoint.LoadElement(Settings.GetHeadElementPrefab(Head));
            UpdateHairColor();
            OnHeadChangedEvent?.Invoke(Head);
        }

        private void UpdateHairColor()
        {
            var material = Settings.GetHairMaterial(Head, HairColor);
            var index = Settings.GetHairMaterialIndex(Head);
            HeadCustomizationPoint.LoadMaterialOnElement(material, index);
            OnHairColorChangedEvent?.Invoke(HairColor);
        }

        private void UpdateEyes()
        {
            FaceCustomizationPoint.LoadElement(Settings.GetFacePrefabForEyes(Eyes));
            NoseCustomizationPoint.LoadElement(Settings.GetNosePrefabForEyes(Eyes));
            LeftEyeCustomizationPoint.LoadElement(Settings.GetLeftEyePrefabForEyes(Eyes));
            RightEyeCustomizationPoint.LoadElement(Settings.GetRightEyePrefabForEyes(Eyes));
            LeftAltEyeCustomizationPoint.LoadElement(Settings.GetAltLeftEyePrefabForEyes(Eyes));
            RightAltEyeCustomizationPoint.LoadElement(Settings.GetAltRightEyePrefabForEyes(Eyes));
            OnEyesChangedEvent?.Invoke(Eyes);
        }

        private void UpdateSkin()
        {
            foreach (var customizer in GetComponentsInChildren<SkinCustomizer>())
            {
                customizer.LoadMaterial(Settings.GetSkin(Skin));
            }

            OnSkinChangedEvent?.Invoke(Skin);
        }

        private void UpdateClothes()
        {
            foreach (var customizer in GetComponentsInChildren<ClothesCustomizer>())
            {
                customizer.Activate(Clothes);
            }

            UpdateSkin();
            UpdateClothesColor();
            OnClothesChangedEvent?.Invoke(Clothes);
        }

        private void UpdateClothesColor()
        {
            foreach (var customizer in GetComponentsInChildren<ClothesColorCustomizer>())
            {
                customizer.LoadMaterial(Settings.GetClothesColor(customizer.TargetArchetype, ClothesColor,
                    customizer.ClothesType));
            }

            OnClothesColorChangedEvent?.Invoke(ClothesColor);
        }

        private static void OnHeadChanged(Changed<CustomizationBase> customisation)
        {
            customisation.Behaviour.UpdateHead();
        }

        private static void OnHairColorChanged(Changed<CustomizationBase> customisation)
        {
            customisation.Behaviour.UpdateHairColor();
        }

        private static void OnEyesChanged(Changed<CustomizationBase> customisation)
        {
            customisation.Behaviour.UpdateEyes();
        }

        private static void OnSkinChanged(Changed<CustomizationBase> customisation)
        {
            customisation.Behaviour.UpdateSkin();
        }

        private static void OnClothesChanged(Changed<CustomizationBase> customisation)
        {
            customisation.Behaviour.UpdateClothes();
        }

        private static void OnClothesColorChanged(Changed<CustomizationBase> customisation)
        {
            customisation.Behaviour.UpdateClothesColor();
        }
    }
}