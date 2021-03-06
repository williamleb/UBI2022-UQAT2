using System.Collections;
using Canvases.Markers;
using Fusion;
using Interfaces;
using Managers.Game;
using Sirenix.OdinInspector;
using UnityEngine;
using Utilities;
using Utilities.Extensions;
using Utilities.Mesh;
using Random = UnityEngine.Random;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Managers.Rooms
{
    public class Room : NetworkBehaviour, IProbabilityObject
    {
        [SerializeField] private float width = 10f;
        [SerializeField] private float height = 10f;
        [SerializeField] private Vector2 offset = new Vector2(-7.2f,-7.2f);
        
        [SerializeField] private SpriteMarkerReceptor roomMarker;
        
        [Tooltip("Relative probability of this particular room to be chosen by the teacher compared to others. If all rooms have a value of 0.5 for this field, they will all have the same probability of being chosen.")]
        [SerializeField, PropertyRange(0.01f, 1f)] private float probability = 0.5f;

        private Vector3 lowerLeftPosition;
        private Vector3 rotationAngles;

        private Coroutine activateRoomMarkerCoroutine;

        public int RoomId => Id.GetHashCode();
        public float Probability => probability;
        
        private Vector2 MiddlePosition => (lowerLeftPosition + new Vector3(width / 2f, 0f, height / 2f)).RotateAround(lowerLeftPosition, rotationAngles).XZ();
        
        public override void Spawned()
        {
            if (RoomManager.HasInstance)
                RoomManager.Instance.RegisterRoom(this);

            if (GameManager.HasInstance)
                GameManager.Instance.OnGameStateChanged += OnGameStateChanged;
            
            UpdatePositionAndRotationCache();
        }

        private void OnGameStateChanged(GameState _)
        {
            UpdatePositionAndRotationCache();   
        }

        [Button("Update")]
        private void UpdatePositionAndRotationCache()
        {
            var thisTransform = transform;
            rotationAngles = new Vector3(0f, thisTransform.eulerAngles.y, 0f);
            lowerLeftPosition = thisTransform.position + offset.V2ToFlatV3().RotateAround(Vector3.up,rotationAngles);
        }

        public override void Despawned(NetworkRunner runner, bool hasState)
        {
            if (RoomManager.HasInstance)
                RoomManager.Instance.UnregisterRoom(this);
            
            if (GameManager.HasInstance)
                GameManager.Instance.OnGameStateChanged -= OnGameStateChanged;
            
            StopActivateRoomMarkerRoutine();
        }

        public void ActivateRoomMarker(float seconds)
        {
            if (!roomMarker)
                return;
            
            StopActivateRoomMarkerRoutine();
            UpdateRoomSpritePosition();

            activateRoomMarkerCoroutine = StartCoroutine(ActivateRoomMarkerRoutine(seconds));
        }

        private void UpdateRoomSpritePosition()
        {
            var middlePosition = MiddlePosition;
            var roomMarkerTransform = roomMarker.transform;
            roomMarkerTransform.position = new Vector3(middlePosition.x, roomMarkerTransform.position.y, middlePosition.y);
        }

        private IEnumerator ActivateRoomMarkerRoutine(float secondsOfActivation)
        {
            roomMarker.Activate();
            yield return Helpers.GetWait(secondsOfActivation);
            roomMarker.Deactivate();

            activateRoomMarkerCoroutine = null;
        }

        private void StopActivateRoomMarkerRoutine()
        {
            if (activateRoomMarkerCoroutine != null)
            {
                StopCoroutine(activateRoomMarkerCoroutine);
                activateRoomMarkerCoroutine = null;
            }
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

            return (boundingPosition + lowerLeftPosition).RotateAround(lowerLeftPosition, rotationAngles);
        }

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            var thisTransform = transform;
            var rotationAnglesGizmo = new Vector3 (0f, thisTransform.eulerAngles.y, 0f);
            var lowerLeftCorner = thisTransform.position + offset.V2ToFlatV3().RotateAround(Vector3.up,rotationAnglesGizmo);
            var upperLeftCorner = lowerLeftCorner + Vector3.forward * height;
            var lowerRightCorner = lowerLeftCorner + Vector3.right * width;
            var upperRightCorner = lowerRightCorner + Vector3.forward * height;

            upperLeftCorner = upperLeftCorner.RotateAround(lowerLeftCorner, rotationAnglesGizmo);
            lowerRightCorner = lowerRightCorner.RotateAround(lowerLeftCorner, rotationAnglesGizmo);
            upperRightCorner = upperRightCorner.RotateAround(lowerLeftCorner, rotationAnglesGizmo);

            const float lineHeight = 10f;
            
            // Draws the room shape
            Gizmos.color = Color.magenta;
            Gizmos.DrawLine(lowerLeftCorner - Vector3.up * lineHeight, lowerLeftCorner + Vector3.up * lineHeight);
            Gizmos.DrawLine(upperLeftCorner - Vector3.up * lineHeight, upperLeftCorner + Vector3.up * lineHeight);
            Gizmos.DrawLine(lowerRightCorner - Vector3.up * lineHeight, lowerRightCorner + Vector3.up * lineHeight);
            Gizmos.DrawLine(upperRightCorner - Vector3.up * lineHeight, upperRightCorner + Vector3.up * lineHeight);

            var roomMesh = MeshUtils.CreateQuadMesh(upperLeftCorner, upperRightCorner, lowerRightCorner, lowerLeftCorner);
            Gizmos.DrawMesh(roomMesh);

            // Draws the room name
            GUIStyle style = new GUIStyle();
            style.normal.textColor = Color.black;
 
            Handles.BeginGUI();
            var middle = (lowerLeftCorner + new Vector3(width / 2f, 0f, lineHeight / 2f)).RotateAround(lowerLeftCorner, rotationAngles);
            var middle2D = HandleUtility.WorldToGUIPoint(middle);
            GUI.Label(new Rect(middle2D.x, middle2D.y, 100, 100), gameObject.name, style);
            Handles.EndGUI();
        }

        [Button]
        private void OnValidate()
        {
            if (roomMarker != null) return;
            foreach (Transform t in transform)
            {
                if(t.TryGetComponent(out SpriteMarkerReceptor marker))
                {
                    roomMarker = marker;
                    break;
                }
            }
        }
#endif
    }
}