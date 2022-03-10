using Fusion;
using System.Collections.Generic;
using System.Linq;
using Scriptables;
using Systems.Network;
using Units.Player;
using UnityEngine;
using Utilities.Singleton;

namespace Systems
{
	public class PlayerSystem : PersistentSingleton<PlayerSystem>
	{
		private const string PREFABS_FOLDER_PATH = "Game";
		
		private GamePrefabs prefabs;

		private List<PlayerEntity> playersEntity = new List<PlayerEntity>();
		private Dictionary<PlayerRef, NetworkRunner> playersJoined = new Dictionary<PlayerRef, NetworkRunner>();

		public List<PlayerEntity> AllPlayers => playersEntity;

		protected override void Awake()
		{
			base.Awake();
			
			LoadPrefabs();
		}
		
		private void LoadPrefabs()
		{
			var prefabResources = Resources.LoadAll<GamePrefabs>(PREFABS_FOLDER_PATH);

			Debug.Assert(prefabResources.Any(), $"An object of type {nameof(GamePrefabs)} should be in the folder {PREFABS_FOLDER_PATH}");
			if (prefabResources.Length > 1)
				Debug.LogWarning($"More than one object of type {nameof(GamePrefabs)} was found in the folder {PREFABS_FOLDER_PATH}. Taking the first one.");

			prefabs = prefabResources.First();
		}

		private void Start()
        {
			LevelSystem.Instance.OnLobbyLoad += SpawnPlayers;
			LevelSystem.Instance.OnGameLoad += SetPlayerPositionToSpawn;
		}

		// Since the NetworkRunner is deleted after a connection error (idk why),
		// called by the runner to re-register actions
		public void SubscribeNetworkEvents()
        {
			NetworkSystem.Instance.OnPlayerJoinedEvent += PlayerJoined;
			NetworkSystem.Instance.OnPlayerLeftEvent += PlayerLeft;
		}
		private void PlayerJoined(NetworkRunner runner, PlayerRef playerRef)
		{
			Debug.Log($"{playerRef} joined.");
			playersJoined.Add(playerRef, runner);

			// TODO
			// - check number of player in game (rejoin?)
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
			runner.Spawn(prefabs.PlayerPrefab,
						new Vector3(0, 0, 0),
						Quaternion.identity,
						playerRef);
		}

		private void SpawnPlayers()
        {
			Debug.Log("Spawning players...");
			playersJoined.ToList().ForEach(
				keyValuePair => SpawnPlayer(keyValuePair.Value, keyValuePair.Key));
		}

		private void SetPlayerPositionToSpawn()
        {
            foreach (PlayerEntity playerEntity in playersEntity)
            {
				playerEntity.gameObject.transform.position = new Vector3(0,0,0);
            }
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