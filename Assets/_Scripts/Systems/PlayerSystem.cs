using Fusion;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
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

		private PlayerEntity localPlayer;
		private readonly List<PlayerEntity> playersEntity = new List<PlayerEntity>();
		private readonly Dictionary<PlayerRef, NetworkRunner> playersJoined = new Dictionary<PlayerRef, NetworkRunner>();

		private PlayerSpawnLocation[] playerSpawnPoints;
		
		[CanBeNull] public PlayerEntity LocalPlayer => localPlayer;
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
			//LevelSystem.Instance.OnGameLoad += SetPlayersPositionToSpawn;
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
			PlayerEntity player = GetPlayerEntity(playerRef);
			RemovePlayer(player);
		}

		private void SpawnPlayer(NetworkRunner runner, PlayerRef playerRef)
		{
			playerSpawnPoints ??= FindObjectsOfType<PlayerSpawnLocation>();
	        
			Debug.Log($"Spawning {playerRef}");
			runner.Spawn(prefabs.PlayerPrefab,
						playerSpawnPoints[playersEntity.Count].transform.position,
						Quaternion.identity,
						playerRef);
		}

		private void SpawnPlayers()
        {
			Debug.Log("Spawning players...");
			playersJoined.ToList().ForEach(
				keyValuePair => SpawnPlayer(keyValuePair.Value, keyValuePair.Key));
		}

		public void SetPlayersPositionToSpawn()
		{
			playerSpawnPoints = FindObjectsOfType<PlayerSpawnLocation>();
			for (int i = 0; i < playersEntity.Count; i++)
			{
				PlayerEntity playerEntity = playersEntity[i];
				playerEntity.gameObject.transform.position = playerSpawnPoints[i].transform.position;
			}
		}

		public PlayerEntity GetPlayerEntity(PlayerRef playerRef)
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
			if (playerEntity.Object.HasInputAuthority)
				localPlayer = playerEntity;
			
			playersEntity.Add(playerEntity);
		}

		public void RemovePlayer(PlayerEntity player)
		{
			if (player == null || !playersEntity.Contains(player))
				return;

			if (player == localPlayer)
				localPlayer = null;
			
			player.TriggerDespawn();
			playersEntity.Remove(player);
			playersJoined.Remove(player.Object.InputAuthority);
			Debug.Log("Player removed " + player.PlayerId);
		}

		public void DespawnAllPlayers()
		{
			localPlayer = null;
			
            foreach (PlayerEntity playerEntity in playersEntity.ToList())
            {
				playerEntity.TriggerDespawn();
				playersEntity.Remove(playerEntity);
				Debug.Log("Player despawn " + playerEntity.PlayerId);
			}
        }

		public void ResetPlayerSystem()
		{
			playersEntity.Clear();
			Debug.Log("Players list from PlayerSystem cleared");
		}
	}
}