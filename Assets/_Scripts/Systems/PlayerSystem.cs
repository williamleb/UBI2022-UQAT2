using Fusion;
using System.Collections.Generic;
using System.Linq;
using Systems.Network;
using Units.Player;
using UnityEngine;
using Utilities.Singleton;

namespace Systems
{
	public class PlayerSystem : PersistentSingleton<PlayerSystem>
	{
		[SerializeField] private NetworkObject playerPrefab;
		private List<PlayerEntity> playersEntity = new List<PlayerEntity>();
		private Dictionary<PlayerRef, NetworkRunner> playersJoined = new Dictionary<PlayerRef, NetworkRunner>();

		public List<PlayerEntity> AllPlayers => playersEntity;

		private void Start()
		{
			NetworkSystem.Instance.OnPlayerJoinedEvent += PlayerJoined;
			NetworkSystem.Instance.OnPlayerLeftEvent += PlayerLeft;

			LevelSystem.Instance.OnLobbyLoad += SpawnPlayers;
			LevelSystem.Instance.OnGameLoad += SpawnPlayers;
		}

		private void PlayerJoined(NetworkRunner runner, PlayerRef playerRef)
		{
			Debug.Log($"{playerRef} joined.");
			playersJoined.Add(playerRef, runner);

            if (LevelSystem.Instance.State != LevelSystem.LevelState.TRANSITION)
            {
				SpawnPlayer(runner, playerRef);
			}
		}

		private void PlayerLeft(NetworkRunner runner, PlayerRef playerRef)
        {
			Debug.Log($"{playerRef} left.");
			PlayerEntity player = Get(playerRef);
			RemovePlayer(player);
		}

		private void SpawnPlayer(NetworkRunner runner, PlayerRef playerRef)
        {
			Debug.Log($"Spawning {playerRef}");
			runner.Spawn(playerPrefab,
						new Vector3(Random.Range(0, 3), 0, Random.Range(0, 3)),
						Quaternion.identity,
						playerRef);
		}

		private void SpawnPlayers()
        {
			Debug.Log("Spawning players...");
			playersJoined.ToList().ForEach(
				keyValuePair => SpawnPlayer(keyValuePair.Value, keyValuePair.Key));
		}

		public PlayerEntity Get(PlayerRef playerRef)
		{
			for (int i = playersEntity.Count - 1; i >= 0; i--)
			{
				if (playersEntity[i] == null || playersEntity[i].Object == null)
				{
					playersEntity.RemoveAt(i);
					Debug.Log("Removing null player");
				}
				else if (playersEntity[i].Object.InputAuthority == playerRef)
					return playersEntity[i];
			}

			return null;
		}

		//Called by the playerEntity on Spawned()
		public void AddPlayer(PlayerEntity playerEntity)
        {
			playersEntity.Add(playerEntity);
		}

		public void RemovePlayer(PlayerEntity player)
		{
			if (player == null || !playersEntity.Contains(player))
				return;

			player.TriggerDespawn();
			playersEntity.Remove(player);
			playersJoined.Remove(player.PlayerID);
			Debug.Log("Player removed " + player.PlayerID);
		}

		//Called between lobby and game
		public void DespawnAllPlayers()
        {
            foreach (PlayerEntity playerEntity in playersEntity.ToList())
            {
				playerEntity.TriggerDespawn();
				playersEntity.Remove(playerEntity);
				Debug.Log("Player despawn " + playerEntity.PlayerID);
			}
        }

		public void ResetPlayerSystem()
		{
			playersEntity.Clear();
			Debug.Log("Players list from PlayerSystem cleared");
		}
	}
}