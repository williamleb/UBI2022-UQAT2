﻿using Fusion;
using Sirenix.OdinInspector;
using Systems.Settings;
using UnityEngine;

namespace Units.Player.Customisation
{
    public class PlayerCustomization : NetworkBehaviour
    {
        [SerializeField, Required] private CustomizationPoint headCustomizationPoint;
        [SerializeField, Required] private CustomizationPoint faceCustomizationPoint;
        [SerializeField, Required] private CustomizationPoint noseCustomizationPoint;
        [SerializeField, Required] private CustomizationPoint leftEyeCustomizationPoint;
        [SerializeField, Required] private CustomizationPoint rightEyeCustomizationPoint;
        [SerializeField, Required] private CustomizationPoint leftAltEyeCustomizationPoint;
        [SerializeField, Required] private CustomizationPoint rightAltEyeCustomizationPoint;

        [SerializeField] private bool showDebugMenu; // TODO Remove
        
        private CustomizationSettings settings;
        
        [Networked(OnChanged = nameof(OnHeadChanged))] private int Head { get; set; }
        [Networked(OnChanged = nameof(OnHairColorChanged))] private int HairColor { get; set; }
        [Networked(OnChanged = nameof(OnEyesChanged))] private int Eyes { get; set; }
        [Networked(OnChanged = nameof(OnSkinChanged))] private int Skin { get; set; }
        [Networked(OnChanged = nameof(OnClothesChanged))] private int Clothes { get; set; }
        [Networked(OnChanged = nameof(OnClothesColorChanged))] private int ClothesColor { get; set; }

        public override void Spawned()
        {
            base.Spawned();
            
            settings = SettingsSystem.CustomizationSettings;
            if (Object.HasInputAuthority) Randomize();
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
        
        public void IncrementClothesColor() => RPC_IncrementClothesColor();
        public void DecrementClothesColor() => RPC_DecrementClothesColor();
        public void Randomize() => RPC_Randomize();

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
        private void RPC_IncrementClothesColor() => ClothesColor = (ClothesColor + 1) % settings.NumberOfClothesColorElements;
        
        [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
        private void RPC_DecrementClothesColor() => ClothesColor = (ClothesColor + settings.NumberOfClothesColorElements - 1) % settings.NumberOfClothesColorElements;

        [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
        private void RPC_Randomize()
        {
            Head = Random.Range(0, settings.NumberOfHeadElements);
            HairColor = Random.Range(0, settings.NumberOfHairColors);
            Eyes = Random.Range(0, settings.NumberOfEyeElements);
        }

        private void UpdateHead()
        {
            Debug.Log($"Applying head element {Head}");
            headCustomizationPoint.LoadElement(settings.GetHeadElementPrefab(Head));
            UpdateHairColor();
        }
        
        private void UpdateHairColor()
        {
            Debug.Log($"Applying hair color {HairColor}");
            var material = settings.GetHairMaterial(Head, HairColor);
            var index = settings.GetHairMaterialIndex(Head);
            headCustomizationPoint.LoadMaterialOnElement(material, index);
        }

        private void UpdateEyes()
        {
            Debug.Log($"Applying eyes {Eyes}");
            faceCustomizationPoint.LoadElement(settings.GetFacePrefabForEyes(Eyes));
            noseCustomizationPoint.LoadElement(settings.GetNosePrefabForEyes(Eyes));
            leftEyeCustomizationPoint.LoadElement(settings.GetLeftEyePrefabForEyes(Eyes));
            rightEyeCustomizationPoint.LoadElement(settings.GetRightEyePrefabForEyes(Eyes));
            leftAltEyeCustomizationPoint.LoadElement(settings.GetAltLeftEyePrefabForEyes(Eyes));
            rightAltEyeCustomizationPoint.LoadElement(settings.GetAltRightEyePrefabForEyes(Eyes));
        }

        private void UpdateSkin()
        {
            Debug.Log($"Applying skin {Skin}");
            foreach (var customizer in GetComponentsInChildren<SkinCustomizer>())
            {
                customizer.LoadMaterial(settings.GetSkin(Skin));
            }
        }
        
        private void UpdateClothes()
        {
            // TODO Update when the player changes class

            Debug.Log($"Applying clothes {Clothes}");
            // TODO
            UpdateSkin();
            UpdateClothesColor();
        }
        
        private void UpdateClothesColor()
        {
            // TODO Change for the whole team

            Debug.Log($"Applying clothes color {ClothesColor}");
            foreach (var customizer in GetComponentsInChildren<ClothesCustomizer>())
            {
                customizer.LoadMaterial(settings.GetClothesColor(ClothesColor));
            }
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
        
        private void OnGUI()
        {
            if (Runner.IsRunning && Object.HasInputAuthority && showDebugMenu)
            {
                if (GUI.Button(new Rect(0, 40, 200, 40), "IncrementHead"))
                {
                    IncrementHead();
                }

                if (GUI.Button(new Rect(0, 80, 200, 40), "DecrementHead"))
                {
                    DecrementHead();
                }

                if (GUI.Button(new Rect(0, 120, 200, 40), "IncrementHairColor"))
                {
                    IncrementHairColor();
                }

                if (GUI.Button(new Rect(0, 160, 200, 40), "DecrementHairColor"))
                {
                    DecrementHairColor();
                }

                if (GUI.Button(new Rect(0, 200, 200, 40), "IncrementEyes"))
                {
                    IncrementEyes();
                }

                if (GUI.Button(new Rect(0, 240, 200, 40), "DecrementEyes"))
                {
                    DecrementEyes();
                }
                
                if (GUI.Button(new Rect(0, 280, 200, 40), "IncrementSkin"))
                {
                    IncrementSkin();
                }

                if (GUI.Button(new Rect(0, 320, 200, 40), "DecrementSkin"))
                {
                    DecrementSkin();
                }
                
                if (GUI.Button(new Rect(0, 360, 200, 40), "IncrementClothesColor"))
                {
                    IncrementClothesColor();
                }

                if (GUI.Button(new Rect(0, 400, 200, 40), "DecrementClothesColor"))
                {
                    DecrementClothesColor();
                }
            }
        }
    }
}