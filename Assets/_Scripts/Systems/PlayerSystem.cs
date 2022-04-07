using System;
using Fusion;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Scriptables;
using Systems.Level;
using Systems.Network;
using Units.Player;
using UnityEngine;
using Utilities.Singleton;

namespace Systems
{
	public class PlayerSystem : PersistentSingleton<PlayerSystem>
	{
		private const string PREFABS_FOLDER_PATH = "Game";

		public event Action OnAnyPlayerReadyChanged;
		public event Action<PlayerEntity> OnLocalPlayerSpawned;
		
		private GamePrefabs prefabs;

		private PlayerEntity localPlayer;
		private readonly List<PlayerEntity> playersEntity = new List<PlayerEntity>();

		private readonly Dictionary<PlayerRef, NetworkRunner>
			playersJoined = new Dictionary<PlayerRef, NetworkRunner>();

		private PlayerSpawnLocation[] playerSpawnPoints = Array.Empty<PlayerSpawnLocation>();

		private List<PlayerRef> playersBeingSpawned = new List<PlayerRef>();

		
		[CanBeNull] public PlayerEntity LocalPlayer => localPlayer;
		public List<PlayerEntity> AllPlayers => playersEntity;
		public int NumberOfPlayers => playersEntity.Count;
		
		protected override void Awake()
		{
			base.Awake();

			LoadPrefabs();
		}

		private void OnEnable()
		{
			LevelSystem.OnMainMenuStartLoad += OnMainMenuLoad; 
			LevelSystem.OnLobbyLoad += OnLobbyLoad;
			NetworkSystem.OnSceneLoadStartEvent += ResetSpawnPoints;
		}

		private void OnDisable()
		{
			if (LevelSystem.HasInstance)
			{
				LevelSystem.OnMainMenuStartLoad -= OnMainMenuLoad;
				LevelSystem.OnLobbyLoad -= OnLobbyLoad;
				NetworkSystem.OnSceneLoadStartEvent -= ResetSpawnPoints;
			}
		}

		private void OnMainMenuLoad()
		{
			ResetPlayerSystem();
		}

		private void OnLobbyLoad()
        {
			SetPlayersPositionToSpawn();
        }

		private void LoadPrefabs()
		{
			var prefabResources = Resources.LoadAll<GamePrefabs>(PREFABS_FOLDER_PATH);

			Debug.Assert(prefabResources.Any(),
				$"An object of type {nameof(GamePrefabs)} should be in the folder {PREFABS_FOLDER_PATH}");
			if (prefabResources.Length > 1)
				Debug.LogWarning(
					$"More than one object of type {nameof(GamePrefabs)} was found in the folder {PREFABS_FOLDER_PATH}. Taking the first one.");

			prefabs = prefabResources.First();
		}

		private void Start()
		{
			LevelSystem.OnLobbyLoad += SpawnPlayers;
			PlayerEntity.OnReadyChanged += TriggerOnAnyPlayerReadyChanged;
        }

		private void OnDestroy()
		{
			LevelSystem.OnLobbyLoad -= SpawnPlayers;
			PlayerEntity.OnReadyChanged -= TriggerOnAnyPlayerReadyChanged;
		}

		private void TriggerOnAnyPlayerReadyChanged()
		{
			OnAnyPlayerReadyChanged?.Invoke();
		}

		// Since the NetworkRunner is deleted after a connection error (idk why),
		// called by the runner to re-register actions
		public void SubscribeNetworkEvents()
		{
			NetworkSystem.Instance.OnPlayerJoinedEvent += PlayerJoined;
			NetworkSystem.Instance.OnPlayerLeftEvent += PlayerLeft;
		}
		public void UnsubscribeNetworkEvents()
		{
			NetworkSystem.Instance.OnPlayerJoinedEvent -= PlayerJoined;
			NetworkSystem.Instance.OnPlayerLeftEvent -= PlayerLeft;
		}

		private void PlayerJoined(NetworkRunner runner, PlayerRef playerRef)
		{
			Debug.Log($"{playerRef} joined.");
			playersJoined.Add(playerRef, runner);

			// TODO
			// - check number of player in game (rejoin?)
			if (LevelSystem.Instance.State != LevelSystem.LevelState.Transition)
			{
				var alreadySpawnedPlayers = GetAlreadySpawnedPlayers();
				if (!alreadySpawnedPlayers.Contains(playerRef))
					SpawnPlayer(runner, playerRef);
			}
		}

		private IEnumerable<PlayerRef> GetAlreadySpawnedPlayers()
		{
			return playersEntity.Select(entity => entity.Object.InputAuthority).Where(player => player);
		}

		private void PlayerLeft(NetworkRunner runner, PlayerRef playerRef)
		{
			Debug.Log($"{playerRef} left.");
			PlayerEntity player = GetPlayerEntity(playerRef);
			RemovePlayer(player);
		}

		private async void SpawnPlayer(NetworkRunner runner, PlayerRef playerRef)
		{
			if (playersBeingSpawned.Contains(playerRef))
				return;
			
			while (playerSpawnPoints.Length == 0)
			{
				await Task.Delay(10);
				playerSpawnPoints = FindObjectsOfType<PlayerSpawnLocation>();
			}

			var spawnPosition = playersEntity.Count < playerSpawnPoints.Length
				? playerSpawnPoints[playersEntity.Count].transform.position
				: Vector3.zero;

			Debug.Log($"Spawning {playerRef}");
			playersBeingSpawned.Add(playerRef);
			runner.Spawn(prefabs.PlayerPrefab,
						spawnPosition,
						Quaternion.identity,
						playerRef);
		}

		private void SpawnPlayers()
        {
			Debug.Log("Spawning players...");
			var alreadySpawnedPlayers = GetAlreadySpawnedPlayers();
			playersJoined.ToList().ForEach(keyValuePair =>
			{
				if (!alreadySpawnedPlayers.Contains(keyValuePair.Key))
					SpawnPlayer(keyValuePair.Value, keyValuePair.Key);
			});
		}

		public async void SetPlayersPositionToSpawn()
		{
			if (!NetworkSystem.Instance.IsHost)
				return;

			Debug.Log("Setting players to spawn position...");
			while (playerSpawnPoints.Length == 0)
			{
				await Task.Delay(10);
				playerSpawnPoints = FindObjectsOfType<PlayerSpawnLocation>();
			}

			foreach (var playerEntity in playersEntity)
			{
				SetPlayerPositionToSpawn(playerEntity);
			}
		}

		public void SetPlayerPositionToSpawn(PlayerEntity playerEntity)
		{
			var indexOfPlayer = playersEntity.IndexOf(playerEntity);
			Debug.Assert(indexOfPlayer != -1);
			playerEntity.gameObject.transform.position = indexOfPlayer < playerSpawnPoints.Length ? playerSpawnPoints[indexOfPlayer].transform.position : Vector3.zero;
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

		public void AddPlayer(PlayerEntity playerEntity)
		{
			if (playerEntity.Object.HasInputAuthority)
            {
				localPlayer = playerEntity;
				OnLocalPlayerSpawned?.Invoke(playerEntity);
			}
			
			playersEntity.Add(playerEntity);
			playersBeingSpawned.Remove(playerEntity.Object.InputAuthority);
		}

		public void RemovePlayer(PlayerEntity player)
		{
			if (player == null || !playersEntity.Contains(player))
				return;

			if (player == localPlayer)
				localPlayer = null;

			PlayerRef playerRef = player.Object.InputAuthority;
			player.TriggerDespawn();
			playersEntity.Remove(player);
			playersJoined.Remove(playerRef);
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
			localPlayer = null;
			playersEntity.Clear();
			playersJoined.Clear();
			ResetSpawnPoints();
			Debug.Log("Players list from PlayerSystem cleared");
		}

		private void ResetSpawnPoints(NetworkRunner _ = null)
		{
			Debug.Log("Reseting spawnpoints");
			playerSpawnPoints = Array.Empty<PlayerSpawnLocation>();
		}
	}
}