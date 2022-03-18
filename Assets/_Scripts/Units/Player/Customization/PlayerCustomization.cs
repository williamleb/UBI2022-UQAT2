using Fusion;
using Sirenix.OdinInspector;
using Systems.Settings;
using UnityEngine;

namespace Units.Player.Customisation
{
    public class PlayerCustomization : NetworkBehaviour
    {
        [SerializeField, Required] private CustomizationPoint headCustomizationPoint;
        
        private CustomizationSettings settings;
        
        [Networked(OnChanged = nameof(OnHeadChanged))] private int Head { get; set; }
        [Networked(OnChanged = nameof(OnHairColorChanged))] private int HairColor { get; set; }
        [Networked] private int Eyes { get; set; }
        [Networked] private int Skin { get; set; }
        [Networked] private int Clothes { get; set; }
        [Networked] private int ClothesColor { get; set; }

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
        private void RPC_Randomize()
        {
            Head = Random.Range(0, settings.NumberOfHeadElements);
            HairColor = Random.Range(0, settings.NumberOfHairColors);
        }

        private void UpdateHead()
        {
            Debug.Log($"Applying head element {Head}");
            headCustomizationPoint.LoadHead(settings.GetHeadElementPrefab(Head));
            UpdateHairColor();
        }
        
        private void UpdateHairColor()
        {
            Debug.Log($"Applying hair color {HairColor}");
            var material = settings.GetHairMaterial(Head, HairColor);
            var index = settings.GetHairMaterialIndex(Head);
            headCustomizationPoint.LoadMaterialOnHead(material, index);
        }

        private static void OnHeadChanged(Changed<PlayerCustomization> customisation)
        {
            customisation.Behaviour.UpdateHead();
        }
        
        private static void OnHairColorChanged(Changed<PlayerCustomization> customisation)
        {
            customisation.Behaviour.UpdateHairColor();
        }
        
        private void OnGUI()
        {
            if (Runner.IsRunning && Object.HasInputAuthority)
            {
                if (GUI.Button(new Rect(0, 120, 200, 40), "IncrementHead"))
                {
                    IncrementHead();
                }
                if (GUI.Button(new Rect(0, 160, 200, 40), "DecrementHead"))
                {
                    DecrementHead();
                }
                if (GUI.Button(new Rect(0, 200, 200, 40), "IncrementHairColor"))
                {
                    IncrementHairColor();
                }
                if (GUI.Button(new Rect(0, 240, 200, 40), "DecrementHairColor"))
                {
                    DecrementHairColor();
                }
            }
        }
    }
}