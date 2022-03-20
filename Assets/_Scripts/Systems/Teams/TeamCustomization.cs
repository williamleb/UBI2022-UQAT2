using System;
using Fusion;
using Systems.Settings;
using Random = UnityEngine.Random;

namespace Systems.Teams
{
    public partial class Team
    {
        public event Action<string> OnNameChanged;
        public event Action<int> OnColorChanged;

        private CustomizationSettings customizationSettings;
        
        [Networked(OnChanged = nameof(OnNetworkNameChanged))] public string Name { get; private set; }
        [Networked(OnChanged = nameof(OnNetworkColorChanged))] public int Color { get; private set; }

        private void CustomizationSpawned()
        {
            customizationSettings = SettingsSystem.CustomizationSettings;
            RandomizeName();
        }
        
        public void RandomizeName() => RPC_RandomizeName();
        public void RandomizeColor() => RPC_RandomizeColor();
        public void IncrementColor() => RPC_IncrementColor();
        public void DecrementColor() => RPC_DecrementColor();

        [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
        private void RPC_RandomizeName()
        {
            Name = TeamSystem.Instance.GetRandomTeamName();
        }
        
        [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
        private void RPC_RandomizeColor()
        {
            var color = Random.Range(0, customizationSettings.NumberOfTeamColors);
            while (IsColorTaken(color))
            {
                color = (color + 1) % customizationSettings.NumberOfTeamColors;
            }

            Color = color;
        }
        
        [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
        private void RPC_IncrementColor()
        {
            var color = (Color + 1) % customizationSettings.NumberOfTeamColors;
            while (IsColorTaken(color))
            {
                color = (color + 1) % customizationSettings.NumberOfTeamColors;
            }

            Color = color;
        }
        
        [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
        private void RPC_DecrementColor()
        {
            var color = (Color + customizationSettings.NumberOfTeamColors - 1) % customizationSettings.NumberOfTeamColors;
            while (IsColorTaken(color))
            {
                color = (color + customizationSettings.NumberOfTeamColors - 1) % customizationSettings.NumberOfTeamColors;;
            }

            Color = color;
        }

        private bool IsColorTaken(int color)
        {
            foreach (var team in TeamSystem.Instance.Teams)
            {
                if (team != this && team.Color == color)
                    return true;
            }

            return false;
        }

        private void UpdateName()
        {
            OnNameChanged?.Invoke(Name);
        }

        private void UpdateColor()
        {
            OnColorChanged?.Invoke(Color);
        }

        private static void OnNetworkNameChanged(Changed<Team> changed)
        {
            changed.Behaviour.UpdateName();
        }
        
        private static void OnNetworkColorChanged(Changed<Team> changed)
        {
            changed.Behaviour.UpdateColor();
        }
    }
}