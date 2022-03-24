using Canvases.Markers;
using Fusion;
using Systems;
using Systems.Network;
using UnityEngine;

namespace Units.Player
{
    public partial class PlayerEntity
    {
        [Header("Ready")] 
        [SerializeField] private TextMarkerReceptor readyMarker;
        
        [Networked(OnChanged = nameof(OnIsReadyChanged)), HideInInspector] public NetworkBool IsReady { get; set; }

        private void ReadyUpdate(NetworkInputData inputData)
        {
            if (Runner.IsForward)
            {
                if (inputData.IsReadyOnce && !inMenu && !InCustomization)
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

        private static void OnIsReadyChanged(Changed<PlayerEntity> changed)
        {
            changed.Behaviour.UpdateReadyMarker();
        }
    }
}