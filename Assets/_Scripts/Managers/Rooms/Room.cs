using Fusion;
using Units.Player;
using UnityEngine;
using Utilities.Extensions;
using Utilities.Mesh;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Managers.Rooms
{
    public class Room : NetworkBehaviour
    {
        [SerializeField] private float width = 10f;
        [SerializeField] private float height = 10f;

        private Vector3 lowerLeftPosition;
        private Vector3 rotationAngles;

        public int RoomId => Id.GetHashCode();
        
        public override void Spawned()
        {
            if (RoomManager.HasInstance)
                RoomManager.Instance.RegisterRoom(this);

            var thisTransform = transform;
            lowerLeftPosition = thisTransform.position;
            rotationAngles = new Vector3(0f, thisTransform.eulerAngles.y, 0f);
        }
        
        public override void Despawned(NetworkRunner runner, bool hasState)
        {
            if (RoomManager.HasInstance)
                RoomManager.Instance.UnregisterRoom(this);
        }

        public bool IsInRoom(Vector3 position)
        {
            var rotatedPosition = position.RotateAround(lowerLeftPosition, -rotationAngles);
            var boundingPosition = rotatedPosition - lowerLeftPosition;

            return boundingPosition.x > 0f && boundingPosition.x < width &&
                   boundingPosition.z > 0f && boundingPosition.z < height;
        }

        public Vector3 GetRandomRoomPosition()
        {
            var randomX = Random.Range(0f, width);
            var randomZ = Random.Range(0f, height);

            var boundingPosition = new Vector3(randomX, transform.position.y, randomZ);

            return boundingPosition.RotateAround(lowerLeftPosition, rotationAngles) + lowerLeftPosition;
        }

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            var thisTransform = transform;
            var lowerLeftCorner = thisTransform.position;
            var upperLeftCorner = lowerLeftCorner + Vector3.forward * height;
            var lowerRightCorner = lowerLeftCorner + Vector3.right * width;
            var upperRightCorner = lowerRightCorner + Vector3.forward * height;
            
            var rotationAnglesGizmo = new Vector3 (0f, thisTransform.eulerAngles.y, 0f);

            upperLeftCorner = upperLeftCorner.RotateAround(lowerLeftCorner, rotationAnglesGizmo);
            lowerRightCorner = lowerRightCorner.RotateAround(lowerLeftCorner, rotationAnglesGizmo);
            upperRightCorner = upperRightCorner.RotateAround(lowerLeftCorner, rotationAnglesGizmo);

            const float HEIGHT = 10f;
            
            // Draws the room shape
            Gizmos.color = Color.magenta;
            Gizmos.DrawLine(lowerLeftCorner - Vector3.up * HEIGHT, lowerLeftCorner + Vector3.up * HEIGHT);
            Gizmos.DrawLine(upperLeftCorner - Vector3.up * HEIGHT, upperLeftCorner + Vector3.up * HEIGHT);
            Gizmos.DrawLine(lowerRightCorner - Vector3.up * HEIGHT, lowerRightCorner + Vector3.up * HEIGHT);
            Gizmos.DrawLine(upperRightCorner - Vector3.up * HEIGHT, upperRightCorner + Vector3.up * HEIGHT);

            var roomMesh = MeshUtils.CreateQuadMesh(upperLeftCorner, upperRightCorner, lowerRightCorner, lowerLeftCorner);
            Gizmos.DrawMesh(roomMesh);

            // Draws the room name
            GUIStyle style = new GUIStyle();
            style.normal.textColor = Color.black;
 
            Handles.BeginGUI();
            var middle = (lowerLeftCorner + new Vector3(width / 2f, 0f, height / 2f)).RotateAround(lowerLeftCorner, rotationAngles);
            var middle2D = HandleUtility.WorldToGUIPoint(middle);
            GUI.Label(new Rect(middle2D.x, middle2D.y, 100, 100), gameObject.name, style);
            Handles.EndGUI();
        }
#endif
    }
}