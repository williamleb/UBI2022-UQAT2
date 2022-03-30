using System;
using Fusion;
using Sirenix.OdinInspector;
using Systems.Settings;
using Units.Player;
using UnityEngine;
using Utilities.Extensions;
using Random = UnityEngine.Random;

namespace Units.Customization
{
    public class PlayerCustomization : CustomizationBase
    {
        public override void Spawned()
        {
            base.Spawned();

            Settings = SettingsSystem.CustomizationSettings;
            if (Object.HasInputAuthority) Randomize();

            // For the clients that joined after the client representing this player
            UpdateAll();

            if (Object.HasInputAuthority)
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
        private void IncrementClothesColor() => RPC_IncrementClothesColor();
        private void DecrementClothesColor() => RPC_DecrementClothesColor();
        public void Randomize(bool randomizeGameplayElements = false) => RPC_Randomize(randomizeGameplayElements);

        [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
        private void RPC_IncrementHead() => Head = (Head + 1) % Settings.NumberOfHeadElements;

        [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
        private void RPC_DecrementHead() =>
            Head = (Head + Settings.NumberOfHeadElements - 1) % Settings.NumberOfHeadElements;

        [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
        private void RPC_IncrementHairColor() => HairColor = (HairColor + 1) % Settings.NumberOfHairColors;

        [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
        private void RPC_DecrementHairColor() =>
            HairColor = (HairColor + Settings.NumberOfHairColors - 1) % Settings.NumberOfHairColors;

        [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
        private void RPC_IncrementEyes() => Eyes = (Eyes + 1) % Settings.NumberOfEyeElements;

        [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
        private void RPC_DecrementEyes() =>
            Eyes = (Eyes + Settings.NumberOfEyeElements - 1) % Settings.NumberOfEyeElements;

        [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
        private void RPC_IncrementSkin() => Skin = (Skin + 1) % Settings.NumberOfSkinElements;

        [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
        private void RPC_DecrementSkin() =>
            Skin = (Skin + Settings.NumberOfSkinElements - 1) % Settings.NumberOfSkinElements;

        [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
        private void RPC_SetClothes(Archetype clothes) => Clothes = clothes;

        [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
        private void RPC_SetClothesColor(int clothesColor) => ClothesColor = clothesColor;

        [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
        private void RPC_IncrementClothesColor() => ClothesColor = (ClothesColor + 1) % Settings.NumberOfTeamColors;

        [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
        private void RPC_DecrementClothesColor() =>
            ClothesColor = (ClothesColor + Settings.NumberOfTeamColors - 1) % Settings.NumberOfTeamColors;

        [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
        private void RPC_Randomize(NetworkBool randomizeGameplayElements)
        {
            Head = Random.Range(0, Settings.NumberOfHeadElements);
            HairColor = Random.Range(0, Settings.NumberOfHairColors);
            Eyes = Random.Range(0, Settings.NumberOfEyeElements);
            Skin = Random.Range(0, Settings.NumberOfSkinElements);

            if (randomizeGameplayElements)
            {
                Clothes = ((Archetype[]) Enum.GetValues(typeof(Archetype))).RandomElement();
                ClothesColor = Random.Range(0, Settings.NumberOfTeamColors);
            }
        }

        [Rpc(RpcSources.InputAuthority, RpcTargets.All)]
        private void RPC_ForceUpdateAll()
        {
            UpdateAll();
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