using System;
using Fusion;
using Sirenix.OdinInspector;
using Systems.Settings;
using UnityEngine;
using Utilities.Extensions;
using Random = UnityEngine.Random;

namespace Units.Player.Customisation
{
    public class PlayerCustomization : NetworkBehaviour
    {
        public event Action<int> OnHeadChangedEvent;
        public event Action<int> OnHairColorChangedEvent;
        public event Action<int> OnEyesChangedEvent;
        public event Action<int> OnSkinChangedEvent;
        public event Action<Archetype> OnClothesChangedEvent;
        public event Action<int> OnClothesColorChangedEvent;

        [SerializeField, Required] private CustomizationPoint headCustomizationPoint;
        [SerializeField, Required] private CustomizationPoint faceCustomizationPoint;
        [SerializeField, Required] private CustomizationPoint noseCustomizationPoint;
        [SerializeField, Required] private CustomizationPoint leftEyeCustomizationPoint;
        [SerializeField, Required] private CustomizationPoint rightEyeCustomizationPoint;
        [SerializeField, Required] private CustomizationPoint leftAltEyeCustomizationPoint;
        [SerializeField, Required] private CustomizationPoint rightAltEyeCustomizationPoint;
        
        private CustomizationSettings settings;
        
        [Networked(OnChanged = nameof(OnHeadChanged))] public int Head { get; private set; }
        [Networked(OnChanged = nameof(OnHairColorChanged))] public int HairColor { get; private set; }
        [Networked(OnChanged = nameof(OnEyesChanged))] public int Eyes { get; private set; }
        [Networked(OnChanged = nameof(OnSkinChanged))] public int Skin { get; private set; }
        [Networked(OnChanged = nameof(OnClothesChanged))] public Archetype Clothes { get; private set; }
        [Networked(OnChanged = nameof(OnClothesColorChanged))] public int ClothesColor { get; private set; }

        public override void Spawned()
        {
            base.Spawned();
            
            settings = SettingsSystem.CustomizationSettings;
            if (Object.HasInputAuthority) Randomize(true);

            if (Object.HasStateAuthority)
            {
                // Necessary, because if elements were randomized to 0, the OnChanged will not be called
                RPC_ForceUpdateAll();
            }
        }
        
        // Only call these methods on input authority
        public void IncrementHead() => RPC_IncrementHead();
        public void DecrementHead() => RPC_DecrementHead();
        public void IncrementHairColor() => RPC_IncrementHairColor();
        public void DecrementHairColor() => RPC_DecrementHairColor();
        public void IncrementEyes() => RPC_IncrementEyes();
        public void DecrementEyes() => RPC_DecrementEyes();
        public void IncrementSkin() => RPC_IncrementSkin();
        public void DecrementSkin() => RPC_DecrementSkin();
        public void SetClothes(Archetype clothes) => RPC_SetClothes(clothes);
        public void SetClothesColor(int clothesColor) => RPC_SetClothesColor(clothesColor);
        public void IncrementClothesColor() => RPC_IncrementClothesColor();
        public void DecrementClothesColor() => RPC_DecrementClothesColor();
        public void Randomize(bool randomizeGameplayElements = false) => RPC_Randomize(randomizeGameplayElements);

        [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
        private void RPC_IncrementHead() => Head = (Head + 1) % settings.NumberOfHeadElements;
        
        [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
        private void RPC_DecrementHead() => Head = (Head + settings.NumberOfHeadElements - 1) % settings.NumberOfHeadElements;
        
        [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
        private void RPC_IncrementHairColor() => HairColor = (HairColor + 1) % settings.NumberOfHairColors;
        
        [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
        private void RPC_DecrementHairColor() => HairColor = (HairColor + settings.NumberOfHairColors - 1) % settings.NumberOfHairColors;
        
        [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
        private void RPC_IncrementEyes() => Eyes = (Eyes + 1) % settings.NumberOfEyeElements;
        
        [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
        private void RPC_DecrementEyes() => Eyes = (Eyes + settings.NumberOfEyeElements - 1) % settings.NumberOfEyeElements;
        
        [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
        private void RPC_IncrementSkin() => Skin = (Skin + 1) % settings.NumberOfSkinElements;
        
        [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
        private void RPC_DecrementSkin() => Skin = (Skin + settings.NumberOfSkinElements - 1) % settings.NumberOfSkinElements;
        
        [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
        private void RPC_SetClothes(Archetype clothes) => Clothes = clothes;
        
        [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
        private void RPC_SetClothesColor(int clothesColor) => ClothesColor = clothesColor;

        [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
        private void RPC_IncrementClothesColor() => ClothesColor = (ClothesColor + 1) % settings.NumberOfTeamColors;
        
        [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
        private void RPC_DecrementClothesColor() => ClothesColor = (ClothesColor + settings.NumberOfTeamColors - 1) % settings.NumberOfTeamColors;

        [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
        private void RPC_Randomize(NetworkBool randomizeGameplayElements)
        {
            Head = Random.Range(0, settings.NumberOfHeadElements);
            HairColor = Random.Range(0, settings.NumberOfHairColors);
            Eyes = Random.Range(0, settings.NumberOfEyeElements);
            Skin = Random.Range(0, settings.NumberOfSkinElements);

            if (randomizeGameplayElements)
            {
                Clothes = ((Archetype[])Enum.GetValues(typeof(Archetype))).RandomElement();
                ClothesColor = Random.Range(0, settings.NumberOfTeamColors);
            }
        }

        [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
        private void RPC_ForceUpdateAll()
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
            headCustomizationPoint.LoadElement(settings.GetHeadElementPrefab(Head));
            UpdateHairColor();
            OnHeadChangedEvent?.Invoke(Head);
        }
        
        private void UpdateHairColor()
        {
            var material = settings.GetHairMaterial(Head, HairColor);
            var index = settings.GetHairMaterialIndex(Head);
            headCustomizationPoint.LoadMaterialOnElement(material, index);
            OnHairColorChangedEvent?.Invoke(HairColor);
        }

        private void UpdateEyes()
        {
            faceCustomizationPoint.LoadElement(settings.GetFacePrefabForEyes(Eyes));
            noseCustomizationPoint.LoadElement(settings.GetNosePrefabForEyes(Eyes));
            leftEyeCustomizationPoint.LoadElement(settings.GetLeftEyePrefabForEyes(Eyes));
            rightEyeCustomizationPoint.LoadElement(settings.GetRightEyePrefabForEyes(Eyes));
            leftAltEyeCustomizationPoint.LoadElement(settings.GetAltLeftEyePrefabForEyes(Eyes));
            rightAltEyeCustomizationPoint.LoadElement(settings.GetAltRightEyePrefabForEyes(Eyes));
            OnEyesChangedEvent?.Invoke(Eyes);
        }

        private void UpdateSkin()
        {
            foreach (var customizer in GetComponentsInChildren<SkinCustomizer>())
            {
                customizer.LoadMaterial(settings.GetSkin(Skin));
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
                customizer.LoadMaterial(settings.GetClothesColor(customizer.TargetArchetype, ClothesColor));
            }
            OnClothesColorChangedEvent?.Invoke(ClothesColor);
        }

        private static void OnHeadChanged(Changed<PlayerCustomization> customisation)
        {
            customisation.Behaviour.UpdateHead();
        }
        
        private static void OnHairColorChanged(Changed<PlayerCustomization> customisation)
        {
            customisation.Behaviour.UpdateHairColor();
        }
        
        private static void OnEyesChanged(Changed<PlayerCustomization> customisation)
        {
            customisation.Behaviour.UpdateEyes(); 
        }
        
        private static void OnSkinChanged(Changed<PlayerCustomization> customisation)
        {
            customisation.Behaviour.UpdateSkin(); 
        }
        
        private static void OnClothesChanged(Changed<PlayerCustomization> customisation)
        {
            customisation.Behaviour.UpdateClothes(); 
        }
        
        private static void OnClothesColorChanged(Changed<PlayerCustomization> customisation)
        {
            customisation.Behaviour.UpdateClothesColor(); 
        }
        
#if UNITY_EDITOR
        private bool showDebugMenu;

        [Button("ToggleDebugMenu")]
        private void ToggleDebugMenu()
        {
            showDebugMenu = !showDebugMenu;
        }
        
        private void OnGUI()
        {
            if (Runner.IsRunning && Object.HasInputAuthority && showDebugMenu)
            {
                if (GUI.Button(new Rect(0, 0, 200, 40), "IncrementHead"))
                {
                    IncrementHead();
                }

                if (GUI.Button(new Rect(0, 40, 200, 40), "DecrementHead"))
                {
                    DecrementHead();
                }

                if (GUI.Button(new Rect(0, 80, 200, 40), "IncrementHairColor"))
                {
                    IncrementHairColor();
                }

                if (GUI.Button(new Rect(0, 120, 200, 40), "DecrementHairColor"))
                {
                    DecrementHairColor();
                }

                if (GUI.Button(new Rect(0, 160, 200, 40), "IncrementEyes"))
                {
                    IncrementEyes();
                }

                if (GUI.Button(new Rect(0, 200, 200, 40), "DecrementEyes"))
                {
                    DecrementEyes();
                }
                
                if (GUI.Button(new Rect(0, 240, 200, 40), "IncrementSkin"))
                {
                    IncrementSkin();
                }

                if (GUI.Button(new Rect(0, 280, 200, 40), "DecrementSkin"))
                {
                    DecrementSkin();
                }
                
                if (GUI.Button(new Rect(0, 320, 200, 40), "IncrementClothesColor"))
                {
                    IncrementClothesColor();
                }

                if (GUI.Button(new Rect(0, 360, 200, 40), "DecrementClothesColor"))
                {
                    DecrementClothesColor();
                }
            }
        }
#endif
    }
}