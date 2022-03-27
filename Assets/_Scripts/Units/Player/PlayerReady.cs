using Canvases.Markers;
using Fusion;
using Systems;
using Systems.Network;
using Systems.Settings;
using Systems.Teams;
using UnityEngine;

namespace Units.Player
{
    public partial class PlayerEntity
    {
        [Header("Ready")] 
        [SerializeField] private TextMarkerReceptor readyMarker;
        
        [Networked(OnChanged = nameof(OnIsReadyChanged)), HideInInspector] public NetworkBool IsReady { get; set; }

        private Team readySubscribedTeam;

        private void InitReady()
        {
            OnTeamChanged += ReadyOnTeamChanged;
        }

        private void ReadyUpdate(NetworkInputData inputData)
        {
            if (!LevelSystem.Instance.IsLobby)
            {
                IsReady = false;
                return;
            }
            
            if (Runner.IsForward)
            {
                if (inputData.IsReadyOnce && !InMenu && !InCustomization)
                {
                    IsReady = !IsReady;
                    Debug.Log($"Toggle ready for player id {PlayerId} : {IsReady}");
                }
            }
        }

        private void ResetReady()
        {
            IsReady = false;
        }

        private void UpdateReadyMarker()
        {
            if (!readyMarker)
                return;
            
            if (IsReady)
                readyMarker.Activate();
            else
                readyMarker.Deactivate();
        }

        private void ReadyOnTeamChanged()
        {
            if (!readyMarker)
                return;

            if (readySubscribedTeam)
            {
                readySubscribedTeam.OnColorChanged -= ReadyOnTeamChangedColor;
            }

            readySubscribedTeam = TeamSystem.Instance.GetTeam(TeamId);
            readySubscribedTeam.OnColorChanged += ReadyOnTeamChangedColor;
            ReadyOnTeamChangedColor(readySubscribedTeam.Color);
        }

        private void ReadyOnTeamChangedColor(int newColor)
        {
            readyMarker.Color = SettingsSystem.CustomizationSettings.GetColor(newColor);
        }

        private static void OnIsReadyChanged(Changed<PlayerEntity> changed)
        {
            changed.Behaviour.UpdateReadyMarker();
        }
    }
}