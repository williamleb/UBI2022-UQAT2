using System.Linq;
using Fusion;
using Scriptables;
using Sirenix.OdinInspector;
using Systems.Network;
using UnityEngine;
using Utilities.Singleton;

namespace Systems.MapGeneration
{
    public class MapGenerationSystem : Singleton<MapGenerationSystem>
    {
        private const string MAP_FOLDER_PATH = "Game";

        private MapLayouts mapLayouts;
        private MapRooms mapRooms;

        protected override void Awake()
        {
            base.Awake();
            LoadLayouts();
            LoadRooms();
        }

        private void LoadLayouts()
        {
            var layouts = Resources.LoadAll<MapLayouts>(MAP_FOLDER_PATH);

            Debug.Assert(layouts.Any(), $"An object of type {nameof(MapLayouts)} should be in the folder {MAP_FOLDER_PATH}");
            if (layouts.Length > 1)
                Debug.LogWarning($"More than one object of type {nameof(MapLayouts)} was found in the folder {MAP_FOLDER_PATH}. Taking the first one.");

            mapLayouts = layouts.First();
        }
        
        private void LoadRooms()
        {
            var rooms = Resources.LoadAll<MapRooms>(MAP_FOLDER_PATH);

            Debug.Assert(rooms.Any(), $"An object of type {nameof(MapRooms)} should be in the folder {MAP_FOLDER_PATH}");
            if (rooms.Length > 1)
                Debug.LogWarning($"More than one object of type {nameof(MapRooms)} was found in the folder {MAP_FOLDER_PATH}. Taking the first one.");

            mapRooms = rooms.First();
        }

        [Button]
        public void GenerateMap()
        {
            MapReference mapReference = mapLayouts.GetRandomMapLayout();
            MapGenerationInfo mapGenerationInfo = mapReference.MapGenerationInfo;
            NetworkObject propPrefab = mapReference.PropPrefab;
            NetworkObject hallways = mapReference.Hallways;
            NetworkObject spawnPoints = mapReference.SpawnPoints;
            foreach (RoomGenerationInfo roomGenerationInfo in mapGenerationInfo.Rooms)
            {
                RoomInfo randomRoom = mapRooms.GetRandomMatchingRoom(roomGenerationInfo);
                Debug.Assert(randomRoom != null , $"Didn't find any matching room for settings {roomGenerationInfo.DoorLayout} - {roomGenerationInfo.RoomSize}");
                Quaternion rotation = CalculateRotation(randomRoom.TopDirection,roomGenerationInfo.DesiredOrientation);
                Vector3 position = roomGenerationInfo.transform.position +
                                   Vector3.up * 0.01f +
                                   Vector3.forward * roomGenerationInfo.Height / 2 +
                                   Vector3.right * roomGenerationInfo.Width / 2;
                NetworkSystem.Instance.Spawn(randomRoom.GetComponent<NetworkObject>(), position, rotation);
            }

            NetworkSystem.Instance.Spawn(propPrefab, propPrefab.transform.position, Quaternion.identity);
            NetworkSystem.Instance.Spawn(hallways, hallways.transform.position, Quaternion.identity);
            NetworkSystem.Instance.Spawn(spawnPoints, spawnPoints.transform.position, Quaternion.identity);
        }

        private Quaternion CalculateRotation(DesiredOrientation randomRoomTopDirection, DesiredOrientation desiredOrientation)
        {
            return Quaternion.Euler(0,desiredOrientation - randomRoomTopDirection,0);
        }
    }
}