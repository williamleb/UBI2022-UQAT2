using System.Linq;
using Scriptables;
using Sirenix.OdinInspector;
using Unity.Mathematics;
using UnityEngine;

namespace Systems.MapGeneration
{
    public class MapGenerationSystem : MonoBehaviour
    {
        private const string MAP_FOLDER_PATH = "Game";

        private MapLayouts mapLayouts;
        private MapRooms mapRooms;
        
        protected void Awake()
        {
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
        private void GenerateMap()
        {
            MapGenerationInfo mapGenerationInfo = mapLayouts.GetRandomMapLayout();
            foreach (RoomGenerationInfo roomGenerationInfo in mapGenerationInfo.Rooms)
            {
                RoomInfo randomRoom = mapRooms.GetRandomMapRoom();
                Quaternion rotation = CalculateRotation(randomRoom.TopDirection,roomGenerationInfo.DesiredOrientation);
                Instantiate(randomRoom, roomGenerationInfo.transform.position, rotation);
            }
        }

        private Quaternion CalculateRotation(DesiredOrientation randomRoomTopDirection, DesiredOrientation desiredOrientation)
        {
            return Quaternion.Euler(0,desiredOrientation - randomRoomTopDirection,0);
        }
    }
}