using System.Linq;
using Scriptables;
using Sirenix.OdinInspector;
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
                RoomInfo randomRoom = mapRooms.GetRandomMatchingRoom(roomGenerationInfo);
                Debug.Assert(randomRoom != null , $"Didn't find any matching room for settings {roomGenerationInfo.DoorLayout} - {roomGenerationInfo.RoomSize}");
                Quaternion rotation = CalculateRotation(randomRoom.TopDirection,roomGenerationInfo.DesiredOrientation);
                Vector3 position = roomGenerationInfo.transform.position +
                                   Vector3.forward * roomGenerationInfo.Height / 2 +
                                   Vector3.right * roomGenerationInfo.Width / 2;
                Instantiate(randomRoom, position, rotation);
            }
        }

        private Quaternion CalculateRotation(DesiredOrientation randomRoomTopDirection, DesiredOrientation desiredOrientation)
        {
            return Quaternion.Euler(0,desiredOrientation - randomRoomTopDirection,0);
        }
    }
}