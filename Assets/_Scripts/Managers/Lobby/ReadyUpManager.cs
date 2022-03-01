using System.Collections;
using Systems;
using UnityEngine;
using Utilities.Singleton;

public class ReadyUpManager : Singleton<ReadyUpManager>
{
    private bool allPlayersReady;
    [SerializeField] private int requiredPlayerToStartGame = 0;

    void Update()
    {
        if (allPlayersReady)
            return;

        if (PlayerSystem.Instance.AllPlayers.Count > requiredPlayerToStartGame)
        {
            allPlayersReady = true;
            StartCoroutine(StartGameCoroutine());
        }
    }

    IEnumerator StartGameCoroutine()
    {
        yield return new WaitForSeconds(5.0f);
        LevelSystem.Instance.LoadGame();
    }
}
