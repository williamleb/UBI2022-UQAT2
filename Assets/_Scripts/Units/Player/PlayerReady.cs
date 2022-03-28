using System;
using Canvases.Markers;
using Fusion;
using Systems;
using Systems.Level;
using Systems.Network;
using Systems.Settings;
using Systems.Teams;
using UnityEngine;

namespace Units.Player
{
    public partial class PlayerEntity
    {
        public static event Action OnReadyChanged; 
        
        [Header("Ready")] 
        [SerializeField] private TextMarkerReceptor readyMarker;
        
        [Networked(OnChanged = nameof(OnIsReadyChanged))] public NetworkBool IsReady { get; set; }

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
                OnReadyChanged?.Invoke();
                return;
            }
            
            if (Runner.IsForward)
            {
                if (inputData.IsReadyOnce && !InMenu && !InCustomization)
                {
                    IsReady = !IsReady;
                    OnReadyChanged?.Invoke();
                    Debug.Log($"Toggle ready for player id {PlayerId} : {IsReady}");
                }
            }
        }

        private void ResetReady()
        {
            IsReady = false;
            OnReadyChanged?.Invoke();
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