using Fusion;
using Sirenix.OdinInspector;
using Systems.Settings;
using UnityEngine;

namespace Units.Player.Customisation
{
    public class PlayerCustomisation : NetworkBehaviour
    {
        private CustomisationSettings settings;
        
        [Networked, OnValueChanged(nameof(OnHeadChanged))] private int Head { get; set; }
        [Networked, OnValueChanged(nameof(OnHairColorChanged))] private int HairColor { get; set; }
        [Networked] private int Eyes { get; set; }
        [Networked] private int Skin { get; set; }
        [Networked] private int Clothes { get; set; }
        [Networked] private int ClothesColor { get; set; }

        private void Start()
        {
            settings = SettingsSystem.CustomisationSettings;
        }

        public override void Spawned()
        {
            base.Spawned();
            // Randomize();
        }
        
        // Only call these methods on input authority
        // public void IncrementHead()
        public void Randomize() => RPC_Randomize();
        
        // private voir RPC_IncrementHead() => Head = (Head + 1) % 

        [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
        private void RPC_Randomize()
        {
            Head = Random.Range(0, settings.NumberOfHeadElements);
            HairColor = Random.Range(0, settings.NumberOfHairColors);
        }

        private void UpdateHead()
        {
            // TODO
        }
        
        private void UpdateHairColor()
        {
            // TODO
        }

        private static void OnHeadChanged(Changed<PlayerCustomisation> customisation)
        {
            customisation.Behaviour.UpdateHead();
        }
        
        private static void OnHairColorChanged(Changed<PlayerCustomisation> customisation)
        {
            customisation.Behaviour.UpdateHairColor();
        }
    }
}