using System;
using Systems;
using UnityEngine;

namespace Ingredients.Volumes
{
    [RequireComponent(typeof(Collider))]
    public class LocalPlayerDetection : MonoBehaviour
    {
        public event Action OnLocalPlayerEntered;
        public event Action OnLocalPlayerLeft;
        
        private Collider col;

        private bool localPlayerIsIn;

        public bool LocalPlayerIsIn
        {
            get => localPlayerIsIn;
            set
            {
                if (value == localPlayerIsIn)
                    return;
                
                localPlayerIsIn = value;
                if (value) OnLocalPlayerEntered?.Invoke();
                else OnLocalPlayerLeft?.Invoke();
            }
        }

        private void Awake()
        {
            col = GetComponent<Collider>();
        }

        private void Update()
        {
            if (!PlayerSystem.HasInstance)
                return;

            var localPlayer = PlayerSystem.Instance.LocalPlayer;
            if (!localPlayer)
                return;

            LocalPlayerIsIn = col.bounds.Contains(localPlayer.transform.position);
        }
    }
}