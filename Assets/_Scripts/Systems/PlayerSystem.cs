using Fusion;
using System.Collections;
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
		}

		private void PlayerJoined(NetworkRunner runner, PlayerRef playerRef)
		{
			playersJoined.Add(playerRef, runner);
		}

		private void PlayerLeft(NetworkRunner runner, PlayerRef playerRef)
        {
			Debug.Log($"{playerRef} despawn.");
			PlayerEntity player = Get(playerRef);
			player.TriggerDespawn();
			RemovePlayer(player);
		}

		private void SpawnPlayers()
        {
			playersJoined.ToList().ForEach(keyValuePair => keyValuePair.Value.Spawn(playerPrefab, new Vector3(0, 0, 5), Quaternion.identity, keyValuePair.Key));
			LevelSystem.Instance.OnLobbyLoad -= SpawnPlayers;
			playersJoined.Clear();
		}

		private void OnLevelChangedPlayerDespawn()
        {

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

			playersEntity.Remove(player);
			Debug.Log("Player removed " + player.PlayerID);
		}

		public void ResetPlayerSystem()
		{
			playersEntity.Clear();
			Debug.Log("Players list from PlayerSystem cleared");
		}
	}
}