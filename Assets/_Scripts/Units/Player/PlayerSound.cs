using System.Collections.Generic;
using Fusion;
using Systems;
using Systems.Sound;
using Units.AI;
using UnityEngine;
using Utilities.Extensions;

namespace Units.Player
{
    [RequireComponent(typeof(AkGameObj))]
    public partial class PlayerEntity : IAudioObject
    {
        private AkGameObj audioObject;
        private AkAudioListener audioListener;

        private readonly List<IAudioObject> listenedObjects = new List<IAudioObject>();
        
        public AkGameObj AudioObject => audioObject;

        public void PlayFootstepSoundLocally() => SoundSystem.Instance.PlayFootstepSound(this);
        public void PlayFumbleSoundLocally() => SoundSystem.Instance.PlayFumbleSound(this);
        public void PlayHandInHomeworkSoundLocally() => SoundSystem.Instance.PlayHandInHomeworkSound(this);
        public void PlayPickUpHomeworkSoundLocally() => SoundSystem.Instance.PlayPickUpHomeworkSound(this);
        public void PlayAimHoldSoundLocally() => SoundSystem.Instance.PlayAimHoldSound(this);
        public void StopAimHoldSoundLocally() => SoundSystem.Instance.StopAimHoldSound(this);
        public void PlayAimReleaseSoundLocally() => SoundSystem.Instance.PlayAimReleaseSound(this);
        public void SetAimSoundChargePercentValueLocally(float value) => SoundSystem.Instance.SetAimCharge(this, value);
        public void PlayDashSoundLocally() => SoundSystem.Instance.PlayDashSound(this);
        public void StopDashSoundLocally() => SoundSystem.Instance.StopDashSound(this);
        public void PlayDashCollisionSoundLocally() => SoundSystem.Instance.PlayDashCollisionSound(this);
        
        public void PlayHandInHomeworkSoundOnOtherClients() => RPC_PlayHandInHomeworkSoundOnOtherClients();
        public void PlayPickUpHomeworkSoundOnOtherClients() => RPC_PlayPickUpHomeworkSoundOnOtherClients();
        public void PlayAimHoldSoundOnOtherClients() => RPC_PlayAimHoldSoundOnOtherClients();
        public void StopAimHoldSoundOnOtherClients() => RPC_StopAimHoldSoundOnOtherClients();
        public void PlayAimReleaseSoundOnOtherClients() => RPC_PlayAimReleaseSoundOnOtherClients();
        public void PlayDashSoundOnOtherClients() => RPC_PlayDashSoundOnOtherClients();

        private void SoundAwake()
        {
            audioObject = GetComponent<AkGameObj>();
        }

        private void InitSound()
        {
            if (!Object.HasInputAuthority)
                return;

            audioListener = gameObject.AddComponent<AkAudioListener>();
            audioListener.isDefaultListener = false;
            
            ListenToExistingObjects();
            SubscribeToAudioObjectsSpawned();
        }

        private void TerminateSound()
        {
            if (!Object.HasInputAuthority)
                return;

            foreach (var listenedObject in listenedObjects)
            {
                audioListener.StopListeningToEmitter(listenedObject.AudioObject);
            }
            
            UnsubscribeToAudioObjectsSpawned();
        }
        
        private void ListenToExistingObjects()
        {
            foreach (var playerEntity in PlayerSystem.Instance.AllPlayers)
            {
                ListenToObject(playerEntity);
            }

            if (AIManager.HasInstance)
            {
                if (AIManager.Instance.Teacher)
                    ListenToObject(AIManager.Instance.Teacher);

                foreach (var janitor in AIManager.Instance.Janitors)
                {
                    ListenToObject(janitor);
                }

                foreach (var student in AIManager.Instance.Students)
                {
                    ListenToObject(student);
                }
            }
        }

        private void SubscribeToAudioObjectsSpawned()
        {
            OnPlayerSpawned += OnPlayerEntitySpawned;
            OnPlayerDespawned += OnPlayerEntityDespawned;
            AIEntity.OnAISpawned += OnAIEntitySpawned;
            AIEntity.OnAIDespawned += OnAIEntityDespawned;
        }
        
        private void UnsubscribeToAudioObjectsSpawned()
        {
            OnPlayerSpawned -= OnPlayerEntitySpawned;
            OnPlayerDespawned -= OnPlayerEntityDespawned;
            AIEntity.OnAISpawned -= OnAIEntitySpawned;
            AIEntity.OnAIDespawned -= OnAIEntityDespawned;
        }

        private void OnPlayerEntitySpawned(NetworkObject playerObject)
        {
            var playerEntity = playerObject.GetComponentInEntity<PlayerEntity>();
            Debug.Assert(playerEntity);
            ListenToObject(playerEntity);
        }
        
        private void OnPlayerEntityDespawned(NetworkObject playerObject)
        {
            var playerEntity = playerObject.GetComponentInEntity<PlayerEntity>();
            Debug.Assert(playerEntity);
            StopListeningToObject(playerEntity);
        }

        private void OnAIEntitySpawned(AIEntity aiEntity)
        {
            ListenToObject(aiEntity);
        }
        
        private void OnAIEntityDespawned(AIEntity aiEntity)
        {
            StopListeningToObject(aiEntity);
        }

        private void ListenToObject(IAudioObject objectToListenTo)
        {
            if (listenedObjects.Contains(objectToListenTo))
                return;
            
            audioListener.StartListeningToEmitter(objectToListenTo.AudioObject);
            listenedObjects.Add(objectToListenTo);
        }

        private void StopListeningToObject(IAudioObject objectToStopListeningTo)
        {
            if (!listenedObjects.Contains(objectToStopListeningTo))
                return;
            
            audioListener.StopListeningToEmitter(objectToStopListeningTo.AudioObject);
            listenedObjects.Remove(objectToStopListeningTo);
        }

        private void PlayDashSound()
        {
            // We separate the 2 calls so it can be played instantly on the client
            if (Object.HasInputAuthority)
                PlayDashSoundLocally();
            if (Object.HasStateAuthority)
                PlayDashSoundOnOtherClients();
        }
        
        [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
        private void RPC_PlayHandInHomeworkSoundOnOtherClients()
        {
            if (Object.HasInputAuthority)
                return;
            
            SoundSystem.Instance.PlayHandInHomeworkSound(this);
        }

        [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
        private void RPC_PlayPickUpHomeworkSoundOnOtherClients()
        {
            if (Object.HasInputAuthority)
                return;
            
            SoundSystem.Instance.PlayPickUpHomeworkSound(this);
        }
        
        [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
        private void RPC_PlayAimHoldSoundOnOtherClients()
        {
            if (Object.HasInputAuthority)
                return;
            
            SoundSystem.Instance.PlayAimHoldSound(this);
        }
        
        [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
        private void RPC_StopAimHoldSoundOnOtherClients()
        {
            if (Object.HasInputAuthority)
                return;
            
            SoundSystem.Instance.StopAimHoldSound(this);
        }
        
        [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
        private void RPC_PlayAimReleaseSoundOnOtherClients()
        {
            if (Object.HasInputAuthority)
                return;
            
            SoundSystem.Instance.PlayAimReleaseSound(this);
        }
        
        [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
        private void RPC_PlayDashSoundOnOtherClients()
        {
            if (Object.HasInputAuthority)
                return;
            
            SoundSystem.Instance.PlayDashSound(this);
        }
    }
}