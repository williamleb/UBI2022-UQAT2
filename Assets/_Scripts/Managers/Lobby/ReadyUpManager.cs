using System.Collections;
using Canvases.Components;
using Canvases.TransitionScreen;
using Systems;
using Systems.Level;
using Systems.Settings;
using Systems.Sound;
using Units.Player;
using UnityEngine;
using Utilities;
using Utilities.Singleton;

namespace Managers.Lobby
{
    [RequireComponent(typeof(ReadyUpNetworkData))]
    public class ReadyUpManager : Singleton<ReadyUpManager>
    {
        private const float BUFFER_SECONDS_TO_WAIT_BEFORE_STARTING_LOBBY = 1f;
        
        private bool allPlayersReady;
        private NetworkSettings data;
        private Coroutine startCoroutine;
        private PlayerSystem playerSystem;
        private ReadyUpNetworkData networkData;

        [SerializeField] private TextUIComponent countdownText;
        [SerializeField] private GameObject readyUpMessage;

        private bool isLoadingGame;

        protected override void Awake()
        {
            base.Awake();
            networkData = GetComponent<ReadyUpNetworkData>();
        }

        private void Start()
        {
            data = SettingsSystem.NetworkSettings;

            countdownText.gameObject.SetActive(false);
            playerSystem = PlayerSystem.Instance;
            
            playerSystem.OnAnyPlayerReadyChanged += UpdateReadyForAll;
            playerSystem.OnAnyPlayerReadyChanged += UpdateReadyUpMessage;
            
            LevelSystem.Instance.OnLobbyLoad += OnLobbyLoaded;
        }

        protected override void OnDestroy()
        {
            if (PlayerSystem.HasInstance)
            {
                playerSystem.OnAnyPlayerReadyChanged -= UpdateReadyForAll;
                playerSystem.OnAnyPlayerReadyChanged -= UpdateReadyUpMessage;
            }

            if (LevelSystem.HasInstance)
            {
                LevelSystem.Instance.OnLobbyLoad -= OnLobbyLoaded;
            }
            
            StopAllCoroutines();
            base.OnDestroy();
        }

        private void OnEnable()
        {
            networkData.OnStartingChanged += UpdateCountdownText;
            networkData.OnTimeChanged += UpdateCountdownTime;
            networkData.OnNumberOfDotsChanged += UpdateCountdownText;
            networkData.OnStartedLoadingGame += ShowTransitionScreen;
        }

        private void OnDisable()
        {
            networkData.OnStartingChanged -= UpdateCountdownText;
            networkData.OnTimeChanged -= UpdateCountdownTime;
            networkData.OnNumberOfDotsChanged -= UpdateCountdownText;
            networkData.OnStartedLoadingGame -= ShowTransitionScreen;
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
            if (!networkData && !networkData.Object && !networkData.Object.HasStateAuthority)
                return;

            if (isLoadingGame)
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

            isLoadingGame = true;

            networkData.NotifyStartedLoadingGame();
            yield return new WaitUntil(() => TransitionScreenSystem.Instance.IsShown);
            
            // The game (GameManager) will manage hiding the transition screen and enabling the player inputs
            LevelSystem.Instance.LoadGame();
            ResetIsReadyAllPlayer();
        }

        private void ShowTransitionScreen()
        {
            TransitionScreenSystem.Instance.Show(SettingsSystem.NetworkSettings.LobbyToGameMessage);
        }

        private void ResetIsReadyAllPlayer()
        {
            foreach (PlayerEntity playerEntity in playerSystem.AllPlayers)
            {
                playerEntity.IsReady = false;
            }
        }
        
        private void OnLobbyLoaded()
        {
            StartCoroutine(HideTransitionScreenRoutine());
        }

        private IEnumerator HideTransitionScreenRoutine()
        {
            yield return Helpers.GetWait(BUFFER_SECONDS_TO_WAIT_BEFORE_STARTING_LOBBY);
            
            TransitionScreenSystem.Instance.Hide();
            yield return new WaitUntil(() => TransitionScreenSystem.Instance.IsHidden);
        }
    }
}
