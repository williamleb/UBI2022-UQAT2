using System.Collections.Generic;
using Managers.Interactions;
using Utilities.Singleton;

namespace Managers.Rooms
{
    public class RoomManager : Singleton<RoomManager>
    {
        private Dictionary<int, Room> rooms = new Dictionary<int, Room>();

        public IEnumerable<Room> Rooms => rooms.Values;

        public void RegisterRoom(Room room)
        {
            rooms.Add(room.RoomId, room);
        }

        public void UnregisterRoom(Room room)
        {
            rooms.Remove(room.RoomId);
        }
    }
}