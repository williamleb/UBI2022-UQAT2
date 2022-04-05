using System;
using System.Collections.Generic;
using System.Linq;
using Fusion;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Units.Player
{
    public partial class PlayerEntity
    {
        [Header("FX")] 
        [SerializeField] private ParticleSystem teamSwapFX;
        [SerializeField, TableList] private List<ArchetypeSwapFX> archetypeSwapFXs = new List<ArchetypeSwapFX>();
        [SerializeField] private ParticleSystem hitFx;

        public void PlayTeamSwapFXLocally(Color color)
        {
            if (!teamSwapFX) 
                return;

            var main = teamSwapFX.main;
            main.startColor = color;
            
            teamSwapFX.Play();
        }
        
        public void PlayArchetypeSwapFXLocally(Archetype archetype)
        {
            var fx = GetFXFor(archetype);
            if (fx) fx.Play();
        }

        public void PlayHitFXLocally()
        {
            if (hitFx) hitFx.Play();
        }

        public void PlayTeamSwapFXOnOtherClients(Color color)
        {
            if (Object.HasStateAuthority)
                RPC_PlayTeamSwapFXOnOtherClients(color);
        }

        public void PlayArchetypeSwapFXOnOtherClients(Archetype archetype)
        {
            if (Object.HasStateAuthority)
                RPC_PlayArchetypeSwapFXOnOtherClients(archetype);
        }

        public void PlayHitFXOnOtherClients()
        {
            if (Object.HasStateAuthority)
                RPC_PlayHitFXOnOtherClients();
        }
        
        [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
        private void RPC_PlayTeamSwapFXOnOtherClients(Color color)
        {
            if (!Object.HasInputAuthority)
                PlayTeamSwapFXLocally(color);
        }
        
        [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
        private void RPC_PlayArchetypeSwapFXOnOtherClients(Archetype archetype)
        {
            if (!Object.HasInputAuthority)
                PlayArchetypeSwapFXLocally(archetype);
        }

        [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
        private void RPC_PlayHitFXOnOtherClients()
        {
            if (!Object.HasInputAuthority)
                PlayHitFXLocally();
        }

        private ParticleSystem GetFXFor(Archetype archetype)
        {
            return (from swapFX in archetypeSwapFXs where swapFX.Archetype == archetype select swapFX.FX).FirstOrDefault();
        }
        
        [Serializable]
        private struct ArchetypeSwapFX
        {
            public Archetype Archetype;
            public ParticleSystem FX;
        }
    }
}