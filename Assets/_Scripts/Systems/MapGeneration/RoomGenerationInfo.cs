using System;
using UnityEngine;

namespace Systems.MapGeneration
{
    public class RoomGenerationInfo : MonoBehaviour
    {
        public DoorLayout DoorLayout => doorLayout;
        public DesiredOrientation DesiredOrientation => desiredOrientation;
        public RoomSize RoomSize => roomSize;
        public RoomFloorColor RoomFloorColor => roomFloorColor;

        [SerializeField] private DoorLayout doorLayout;
        [SerializeField] private DesiredOrientation desiredOrientation;
        [SerializeField] private RoomSize roomSize;
        [SerializeField] private RoomFloorColor roomFloorColor;
        [SerializeField] private bool largeRoomOrientation;


        private void OnDrawGizmos()
        {
            var height = roomSize == RoomSize.Lr ? largeRoomOrientation? 15f:30f : 15f;
            var width = roomSize == RoomSize.Sr ? 15f : largeRoomOrientation? 30f:15f;

            var size = new Vector3(width, 0, height);
            var pos = transform.position + Vector3.up * 0.01f;
            switch (roomFloorColor)
            {
                case RoomFloorColor.Floor:
                    Gizmos.color = new Color(192f/255f, 200f/255f, 162f/255f);
                    break;
                case RoomFloorColor.Floor01:
                    Gizmos.color = new Color(115f/255f, 142f/255f, 126f/255f);
                    break;
                case RoomFloorColor.Floor02:
                    Gizmos.color = new Color(209f/255f, 170f/255f, 112f/255f);
                    break;
                case RoomFloorColor.Floor03:
                    Gizmos.color = new Color(140f/255f, 165f/255f, 195f/255f);
                    break;
                case RoomFloorColor.Floor04:
                    Gizmos.color = new Color(143f/255f, 124f/255f, 90f/255f);
                    break;
                case RoomFloorColor.Floor05:
                    Gizmos.color = new Color(115f/255f, 142f/255f, 126f/255f);
                    break;
                case RoomFloorColor.Floor06:
                    Gizmos.color = new Color(140f/255f, 86f/255f, 90f/255f);
                    break;
                case RoomFloorColor.Floor07:
                    Gizmos.color = new Color(46f/255f,45f/255f,44f/255f);
                    break;
                case RoomFloorColor.Floor08:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            Gizmos.DrawCube(pos, size);

            Gizmos.color = Color.black;
            Gizmos.DrawWireCube(pos, size);

            var doorSize = 2.5f;
            var doorPos = new Vector3(doorSize, 0, doorSize);

            switch (doorLayout)
            {
                case DoorLayout.DL1D:
                    Gizmos.color = Color.green;
                    switch (desiredOrientation)
                    {
                        case DesiredOrientation.Top:
                            Gizmos.DrawCube(pos + Vector3.forward * (height / 2 - doorSize / 2), doorPos);
                            break;
                        case DesiredOrientation.Bottom:
                            Gizmos.DrawCube(pos + Vector3.back * (height / 2 - doorSize / 2), doorPos);
                            break;
                        case DesiredOrientation.Left:
                            Gizmos.DrawCube(pos + Vector3.left * (width / 2 - doorSize / 2), doorPos);
                            break;
                        case DesiredOrientation.Right:
                            Gizmos.DrawCube(pos + Vector3.right * (width / 2 - doorSize / 2), doorPos);
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }

                    Gizmos.color = Color.black;
                    break;
                case DoorLayout.DL2Dpar:
                    switch (desiredOrientation)
                    {
                        case DesiredOrientation.Top:
                            Gizmos.color = Color.green;
                            Gizmos.DrawCube(pos + Vector3.forward * (height / 2 - doorSize / 2), doorPos);
                            Gizmos.color = Color.black;
                            Gizmos.DrawCube(pos + Vector3.back * (height / 2 - doorSize / 2), doorPos);
                            break;
                        case DesiredOrientation.Bottom:
                            Gizmos.DrawCube(pos + Vector3.forward * (height / 2 - doorSize / 2), doorPos);
                            Gizmos.color = Color.green;
                            Gizmos.DrawCube(pos + Vector3.back * (height / 2 - doorSize / 2), doorPos);
                            Gizmos.color = Color.black;
                            break;
                        case DesiredOrientation.Left:
                            Gizmos.DrawCube(pos + Vector3.right * (width / 2 - doorSize / 2), doorPos);
                            Gizmos.color = Color.green;
                            Gizmos.DrawCube(pos + Vector3.left * (width / 2 - doorSize / 2), doorPos);
                            Gizmos.color = Color.black;
                            break;
                        case DesiredOrientation.Right:
                            Gizmos.color = Color.green;
                            Gizmos.DrawCube(pos + Vector3.right * (width / 2 - doorSize / 2), doorPos);
                            Gizmos.color = Color.black;
                            Gizmos.DrawCube(pos + Vector3.left * (width / 2 - doorSize / 2), doorPos);
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }

                    break;
                case DoorLayout.DL2Dper:
                    switch (desiredOrientation)
                    {
                        case DesiredOrientation.Top:
                            Gizmos.color = Color.green;
                            Gizmos.DrawCube(pos + Vector3.forward * (height / 2 - doorSize / 2), doorPos);
                            Gizmos.color = Color.black;
                            Gizmos.DrawCube(pos + Vector3.right * (width / 2 - doorSize / 2), doorPos);
                            break;
                        case DesiredOrientation.Bottom:
                            Gizmos.color = Color.green;
                            Gizmos.DrawCube(pos + Vector3.back * (height / 2 - doorSize / 2), doorPos);
                            Gizmos.color = Color.black;
                            Gizmos.DrawCube(pos + Vector3.left * (width / 2 - doorSize / 2), doorPos);
                            break;
                        case DesiredOrientation.Left:
                            Gizmos.DrawCube(pos + Vector3.forward * (height / 2 - doorSize / 2), doorPos);
                            Gizmos.color = Color.green;
                            Gizmos.DrawCube(pos + Vector3.left * (width / 2 - doorSize / 2), doorPos);
                            Gizmos.color = Color.black;
                            break;
                        case DesiredOrientation.Right:
                            Gizmos.DrawCube(pos + Vector3.back * (height / 2 - doorSize / 2), doorPos);
                            Gizmos.color = Color.green;
                            Gizmos.DrawCube(pos + Vector3.right * (width / 2 - doorSize / 2), doorPos);
                            Gizmos.color = Color.black;
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }

                    break;
                case DoorLayout.DL3D:
                    switch (desiredOrientation)
                    {
                        case DesiredOrientation.Top:
                            Gizmos.color = Color.green;
                            Gizmos.DrawCube(pos + Vector3.forward * (height / 2 - doorSize / 2), doorPos);
                            Gizmos.color = Color.black;
                            Gizmos.DrawCube(pos + Vector3.right * (width / 2 - doorSize / 2), doorPos);
                            Gizmos.DrawCube(pos + Vector3.left * (width / 2 - doorSize / 2), doorPos);
                            break;
                        case DesiredOrientation.Bottom:
                            Gizmos.color = Color.green;
                            Gizmos.DrawCube(pos + Vector3.back * (height / 2 - doorSize / 2), doorPos);
                            Gizmos.color = Color.black;
                            Gizmos.DrawCube(pos + Vector3.left * (width / 2 - doorSize / 2), doorPos);
                            Gizmos.DrawCube(pos + Vector3.right * (width / 2 - doorSize / 2), doorPos);
                            break;
                        case DesiredOrientation.Left:
                            Gizmos.DrawCube(pos + Vector3.forward * (height / 2 - doorSize / 2), doorPos);
                            Gizmos.DrawCube(pos + Vector3.back * (height / 2 - doorSize / 2), doorPos);
                            Gizmos.color = Color.green;
                            Gizmos.DrawCube(pos + Vector3.left * (width / 2 - doorSize / 2), doorPos);
                            Gizmos.color = Color.black;
                            break;
                        case DesiredOrientation.Right:
                            Gizmos.DrawCube(pos + Vector3.forward * (height / 2 - doorSize / 2), doorPos);
                            Gizmos.DrawCube(pos + Vector3.back * (height / 2 - doorSize / 2), doorPos);
                            Gizmos.color = Color.green;
                            Gizmos.DrawCube(pos + Vector3.right * (width / 2 - doorSize / 2), doorPos);
                            Gizmos.color = Color.black;
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }

                    break;
                case DoorLayout.DL4D:
                    switch (desiredOrientation)
                    {
                        case DesiredOrientation.Top:
                            Gizmos.color = Color.green;
                            Gizmos.DrawCube(pos + Vector3.forward * (height / 2 - doorSize / 2), doorPos);
                            Gizmos.color = Color.black;
                            Gizmos.DrawCube(pos + Vector3.back * (height / 2 - doorSize / 2), doorPos);
                            Gizmos.DrawCube(pos + Vector3.right * (width / 2 - doorSize / 2), doorPos);
                            Gizmos.DrawCube(pos + Vector3.left * (width / 2 - doorSize / 2), doorPos);
                            break;
                        case DesiredOrientation.Bottom:
                            Gizmos.DrawCube(pos + Vector3.forward * (height / 2 - doorSize / 2), doorPos);
                            Gizmos.color = Color.green;
                            Gizmos.DrawCube(pos + Vector3.back * (height / 2 - doorSize / 2), doorPos);
                            Gizmos.color = Color.black;
                            Gizmos.DrawCube(pos + Vector3.right * (width / 2 - doorSize / 2), doorPos);
                            Gizmos.DrawCube(pos + Vector3.left * (width / 2 - doorSize / 2), doorPos);
                            break;
                        case DesiredOrientation.Left:
                            Gizmos.DrawCube(pos + Vector3.forward * (height / 2 - doorSize / 2), doorPos);
                            Gizmos.DrawCube(pos + Vector3.back * (height / 2 - doorSize / 2), doorPos);
                            Gizmos.DrawCube(pos + Vector3.right * (width / 2 - doorSize / 2), doorPos);
                            Gizmos.color = Color.green;
                            Gizmos.DrawCube(pos + Vector3.left * (width / 2 - doorSize / 2), doorPos);
                            Gizmos.color = Color.black;
                            break;
                        case DesiredOrientation.Right:
                            Gizmos.DrawCube(pos + Vector3.forward * (height / 2 - doorSize / 2), doorPos);
                            Gizmos.DrawCube(pos + Vector3.back * (height / 2 - doorSize / 2), doorPos);
                            Gizmos.color = Color.green;
                            Gizmos.DrawCube(pos + Vector3.right * (width / 2 - doorSize / 2), doorPos);
                            Gizmos.color = Color.black;
                            Gizmos.DrawCube(pos + Vector3.left * (width / 2 - doorSize / 2), doorPos);
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }

    public enum DoorLayout
    {
        DL1D,
        DL2Dpar,
        DL2Dper,
        DL3D,
        DL4D
    }

    public enum DesiredOrientation
    {
        Top = 0,
        Bottom = 180,
        Left = 270,
        Right = 90
    }

    public enum RoomSize
    {
        Sr,
        Lr
    }

    public enum RoomFloorColor
    {
        Floor,
        Floor01,
        Floor02,
        Floor03,
        Floor04,
        Floor05,
        Floor06,
        Floor07,
        Floor08,
    }
}