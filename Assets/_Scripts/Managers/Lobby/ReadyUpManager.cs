using System;
using System.Collections;
using Systems;
using Systems.Level;
using Systems.Settings;
using TMPro;
using Units.Player;
using UnityEngine;
using Utilities;
using Utilities.Singleton;

namespace Managers.Lobby
{
    public class ReadyUpManager : Singleton<ReadyUpManager>
    {
        private bool allPlayersReady;
        private MatchmakingSettings data;
        private Coroutine startCoroutine;
        private PlayerSystem playerSystem;

        [SerializeField] private TextMeshProUGUI countdownText;
        [SerializeField] private GameObject readyUpMessage;

        private void Start()
        {
            data = SettingsSystem.MatchmakingSettings;

            countdownText.gameObject.SetActive(false);
            playerSystem = PlayerSystem.Instance;
        }

        private void Update()
        {
            allPlayersReady = playerSystem.AllPlayers.Count > 1 || (playerSystem.AllPlayers.Count == 1 && data.AllowSoloPlay);

            foreach (PlayerEntity playerEntity in playerSystem.AllPlayers)
            {
                if (playerEntity.Object && playerEntity.Object.HasInputAuthority)
                {
                    readyUpMessage.SetActive(!playerEntity.IsReady);
                }

                if (!playerEntity.IsReady)
                    allPlayersReady = false;
            }

            if (startCoroutine != null && !allPlayersReady)
            {
                countdownText.gameObject.SetActive(false);
                StopCoroutine(startCoroutine);
                startCoroutine = null;
            }

            if (startCoroutine == null && allPlayersReady)
            {
                countdownText.gameObject.SetActive(true);
                startCoroutine = StartCoroutine(StartGameCoroutine());
            }
        }

        IEnumerator StartGameCoroutine()
        {
            for (int i = data.CountDownTime; i > 0; i--)
            {
                countdownText.text = $"{data.CountDownMessage} {i}.";
                yield return Helpers.GetWait(0.25f);

                countdownText.text += ".";
                yield return Helpers.GetWait(0.25f);

                countdownText.text += ".";
                yield return Helpers.GetWait(0.25f);

                countdownText.text += ".";
                yield return Helpers.GetWait(0.25f);
            }

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
