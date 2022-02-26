using Fusion;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
			LevelSystem.Instance.OnLevelLoad += SpawnPlayers;
		}

		private void PlayerJoined(NetworkRunner runner, PlayerRef playerRef)
		{
			Debug.Log($"{playerRef} joined.");
			playersJoined.Add(playerRef, runner);

			//If the game has already started, we spawn the player immediately.
			//This part will depend on how we want to handle the reconnection and the stage system.
			if (LevelSystem.Instance.State != LevelSystem.LevelState.OTHER)
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
			Debug.Log($"{playerRef} spawn.");
			runner.Spawn(playerPrefab, null, null, playerRef);
		}

		private void SpawnPlayers()
        {
			Debug.Log("Spawning players.");
			playersJoined.ToList().ForEach(keyValuePair => keyValuePair.Value.Spawn(playerPrefab, new Vector3(0, 0, 0), Quaternion.identity, keyValuePair.Key));
			playersJoined.Clear();
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

		public void AddPlayers(PlayerEntity player)
		{
			playersEntity.Add(player);
			Debug.Log("Player added " + player.PlayerID);
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

		public void ResetPlayerSystem()
		{
			playersEntity.Clear();
			Debug.Log("Players list from PlayerSystem cleared");
		}
	}
}