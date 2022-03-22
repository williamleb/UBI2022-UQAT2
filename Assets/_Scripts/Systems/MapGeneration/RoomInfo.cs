using UnityEngine;

namespace Systems.MapGeneration
{
    public class RoomInfo : MonoBehaviour
    {
        public DoorLayout DoorLayout => doorLayout;
        public RoomSize RoomSize => roomSize;
        public DesiredOrientation TopDirection => topDirection;

        [SerializeField] private DoorLayout doorLayout;
        [SerializeField] private RoomSize roomSize;
        [SerializeField] private DesiredOrientation topDirection;
    }
}