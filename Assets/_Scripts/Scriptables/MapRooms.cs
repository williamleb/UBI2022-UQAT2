using System.Collections.Generic;
using Systems.MapGeneration;
using UnityEngine;
using Utilities.Extensions;

namespace Scriptables
{
    [CreateAssetMenu(menuName = "Game/Create MapRooms", fileName = "MapRooms", order = 0)]
    public class MapRooms : ScriptableObject
    {
        [SerializeField] private RoomInfo[] mapRooms;

        public RoomInfo GetRandomMapRoom()
        {
            return mapRooms.RandomElement();
        }

        private readonly Dictionary<(DoorLayout, RoomSize), List<RoomInfo>> matchingRooms =
            new Dictionary<(DoorLayout, RoomSize), List<RoomInfo>>();

        public RoomInfo GetRandomMatchingRoom(RoomGenerationInfo roomGenerationInfo)
        {
            var key = (roomGenerationInfo.DoorLayout, roomGenerationInfo.RoomSize);
            if (matchingRooms.TryGetValue(key, out List<RoomInfo> value)) return value.RandomElement();

            List<RoomInfo> rooms = new List<RoomInfo>();

            foreach (RoomInfo roomInfo in mapRooms)
            {
                if (key.DoorLayout == roomInfo.DoorLayout
                    && key.RoomSize == roomInfo.RoomSize)
                {
                    rooms.Add(roomInfo);
                }
            }

            matchingRooms.Add(key, rooms);

            return rooms.RandomElement();
        }
    }
}