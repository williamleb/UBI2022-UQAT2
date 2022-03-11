using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Systems;
using TMPro;
using Units.Player;
using UnityEngine;
using Utilities.Singleton;

public class ReadyUpManager : Singleton<ReadyUpManager>
{
    private bool allPlayersReady;
    [SerializeField] private int countDownSeconds = 10;
    [SerializeField] private TextMeshProUGUI countdownText;
    [SerializeField] private bool allowSoloPlay = true;

    [SerializeField] private GameObject readyUpMessage;

    private Coroutine startCoroutine;

    private PlayerSystem playerSystem;

    private void Start()
    {
        countdownText.gameObject.SetActive(false);
        playerSystem = PlayerSystem.Instance;
    }

    void Update()
    {
        allPlayersReady = playerSystem.AllPlayers.Count > 1 || (playerSystem.AllPlayers.Count == 1 && allowSoloPlay);

        foreach (PlayerEntity playerEntity in playerSystem.AllPlayers)
        {
            if (playerEntity.Object.HasInputAuthority)
            {
                if (playerEntity.IsReady)
                    readyUpMessage.SetActive(false);
                else
                    readyUpMessage.SetActive(true);
            }

            if (!playerEntity.IsReady)
                allPlayersReady = false;
        }

        if (startCoroutine != null && !allPlayersReady)
        {
            countdownText.gameObject.SetActive(false);
            StopCoroutine(startCoroutine);
            startCoroutine = null;
            ResetIsReadyAllPlayer();
        }

        if (startCoroutine == null && allPlayersReady)
        {
            countdownText.gameObject.SetActive(true);
            startCoroutine = StartCoroutine(StartGameCoroutine());
        }
    }

    IEnumerator StartGameCoroutine()
    {
        for (int i = countDownSeconds; i > 0; i--)
        {
            countdownText.text = $"Game starts in {i}.";
            yield return new WaitForSeconds(0.25f);

            countdownText.text += $".";
            yield return new WaitForSeconds(0.25f);

            countdownText.text += $".";
            yield return new WaitForSeconds(0.25f);

            countdownText.text += $".";
            yield return new WaitForSeconds(0.25f);
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
