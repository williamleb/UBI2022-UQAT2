using System;
using System.Collections;
using Canvases.Components;
using Systems;
using Systems.Level;
using Systems.Network;
using Systems.Settings;
using Systems.Sound;
using TMPro;
using Units.Player;
using UnityEngine;
using Utilities;
using Utilities.Singleton;
using Object = System.Object;

namespace Managers.Lobby
{
    [RequireComponent(typeof(ReadyUpNetworkData))]
    public class ReadyUpManager : Singleton<ReadyUpManager>
    {
        private bool allPlayersReady;
        private MatchmakingSettings data;
        private Coroutine startCoroutine;
        private PlayerSystem playerSystem;
        private ReadyUpNetworkData networkData;

        [SerializeField] private TextUIComponent countdownText;
        [SerializeField] private GameObject readyUpMessage;

        protected override void Awake()
        {
            base.Awake();
            networkData = GetComponent<ReadyUpNetworkData>();
        }

        private void Start()
        {
            data = SettingsSystem.MatchmakingSettings;

            countdownText.gameObject.SetActive(false);
            playerSystem = PlayerSystem.Instance;
            
            playerSystem.OnAnyPlayerReadyChanged += UpdateReadyForAll;
            playerSystem.OnAnyPlayerReadyChanged += UpdateReadyUpMessage;
        }

        protected override void OnDestroy()
        {
            if (PlayerSystem.HasInstance)
            {
                playerSystem.OnAnyPlayerReadyChanged -= UpdateReadyForAll;
                playerSystem.OnAnyPlayerReadyChanged -= UpdateReadyUpMessage;
            }
        }

        private void OnEnable()
        {
            networkData.OnStartingChanged += UpdateCountdownText;
            networkData.OnTimeChanged += UpdateCountdownTime;
            networkData.OnNumberOfDotsChanged += UpdateCountdownText;
        }

        private void OnDisable()
        {
            networkData.OnStartingChanged -= UpdateCountdownText;
            networkData.OnTimeChanged -= UpdateCountdownTime;
            networkData.OnNumberOfDotsChanged -= UpdateCountdownText;
        }

        private void UpdateCountdownTime()
        {
            if (networkData.IsStarting)
                PlayCountdownSound();
            
            UpdateCountdownText();
        }

        private void PlayCountdownSound()
        {
            switch (networkData.Time)
            {
                case 0:
                    SoundSystem.Instance.PlayGoSound();
                    break;
                case 1:
                    SoundSystem.Instance.PlayOneSound();
                    break;
                case 2:
                    SoundSystem.Instance.PlayTwoSound();
                    break;
                default:
                    SoundSystem.Instance.PlayThreeSound();
                    break;
            }
        }

        private void UpdateCountdownText()
        {
            countdownText.SetActive(networkData.IsStarting);
            countdownText.Text = $"{data.CountDownMessage} {networkData.Time}.";
            for (var i = 0; i < networkData.NumberOfDots; ++i)
            {
                countdownText.Text += ".";
            }
        }

        private void UpdateReadyUpMessage()
        {
            if (!(LevelSystem.HasInstance && LevelSystem.Instance.IsLobby)) return;
            foreach (var playerEntity in playerSystem.AllPlayers)
            {
                if (playerEntity.Object && playerEntity.Object.HasInputAuthority)
                {
                    readyUpMessage.SetActive(!playerEntity.IsReady);
                }
            }
        }

        private void UpdateReadyForAll()
        {
            if (!networkData.Object.HasStateAuthority)
                return;
            
            if (!(LevelSystem.HasInstance && LevelSystem.Instance.IsLobby)) 
                return;
            
            allPlayersReady = playerSystem.AllPlayers.Count > 1 || (playerSystem.AllPlayers.Count == 1 && data.AllowSoloPlay);
            foreach (var playerEntity in playerSystem.AllPlayers)
            {
                if (!playerEntity.IsReady)
                    allPlayersReady = false;
            }

            if (startCoroutine != null && !allPlayersReady)
            {
                networkData.Revert(data.CountDownTime);
                StopCoroutine(startCoroutine);
                startCoroutine = null;
            }

            if (startCoroutine == null && allPlayersReady)
            {
                networkData.IsStarting = true;
                startCoroutine = StartCoroutine(StartGameCoroutine());
            }
        }

        private IEnumerator StartGameCoroutine()
        {
            for (var i = data.CountDownTime; i > 0; i--)
            {
                networkData.Time = i;
                networkData.NumberOfDots = 0;
                yield return Helpers.GetWait(0.25f);

                networkData.NumberOfDots = 1;
                yield return Helpers.GetWait(0.25f);

                networkData.NumberOfDots = 2;
                yield return Helpers.GetWait(0.25f);

                networkData.NumberOfDots = 3;
                yield return Helpers.GetWait(0.25f);
            }
            networkData.Time = 0;

            LevelSystem.Instance.LoadGame();

            ResetIsReadyAllPlayer();
        }

        private void ResetIsReadyAllPlayer()
        {
            foreach (PlayerEntity playerEntity in playerSystem.AllPlayers)
            {
                playerEntity.IsReady = false;
            }
        }
    }
}
