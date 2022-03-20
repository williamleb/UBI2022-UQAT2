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
    }
}