using Cinemachine;
using UnityEditor;
using UnityEngine;
using Utilities.Extensions;
using Utilities.Unity;

namespace Units.Camera
{
    public class CameraBounds : MonoBehaviour
    {
        [Header("Boundaries")] [SerializeField]
        private Vector3 bounds = Vector3.one;

        [SerializeField] private Vector3 boundsOffset = Vector3.zero;

        private UnityEngine.Camera cam;

        private bool initialized = false;

        public Vector3 Bounds
        {
            get => bounds;
            set => bounds = value;
        }

        private void Awake() => Initialize();

        private void Initialize()
        {
            bounds.y = 0;
            cam = UnityEngine.Camera.main;
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(boundsOffset, bounds * 2);
        }

        public Vector3 StayWithinBounds(CinemachineVirtualCamera playerCamera, Vector3 position, float cameraTiltAngle, float distance)
        {
            if (!initialized)
                Initialize();
            
            float horizontalFOV = playerCamera.m_Lens.FieldOfView;
            float verticalFOV =
                2 * Mathf.Atan(Mathf.Tan(horizontalFOV * Mathf.Deg2Rad / 2) * cam.pixelHeight /
                               cam.pixelWidth) * Mathf.Rad2Deg;

            float allowedXAngle = 90 - horizontalFOV / 2;
            float allowedZAngleLower = cameraTiltAngle + verticalFOV / 2;
            float allowedZAngleUpper = cameraTiltAngle - verticalFOV / 2;

            //Limit the x-left movement
            float xLeftLimit = -bounds.x + boundsOffset.x + Mathf.Cos(allowedXAngle * Mathf.Deg2Rad) * distance;
            if (position.x < xLeftLimit)
                position.x = xLeftLimit;

            //Limit the x-right movement
            float xRightLimit = bounds.x + boundsOffset.x - Mathf.Cos(allowedXAngle * Mathf.Deg2Rad) * distance;
            if (position.x > xRightLimit)
                position.x = xRightLimit;

            float zUpperLimit = bounds.z + boundsOffset.z - Mathf.Cos(allowedZAngleUpper * Mathf.Deg2Rad) * distance -
                                playerCamera.transform.localPosition.z;
            if (position.z > zUpperLimit)
                position.z = zUpperLimit;


            float zLowerLimit = -bounds.z + boundsOffset.z - Mathf.Cos(allowedZAngleLower * Mathf.Deg2Rad) * distance -
                                playerCamera.transform.localPosition.z;
            if (position.z < zLowerLimit)
                position.z = zLowerLimit;

            return position;
        }
        
#if UNITY_EDITOR
        private void OnValidate()
        {
            if (!Application.isPlaying)
                EditorApplication.delayCall += AssignTagAndLayer;
        }

        private void AssignTagAndLayer()
        {
            if (this == null)
                return;

            var thisGameObject = gameObject;

            if (!thisGameObject.AssignTagIfDoesNotHaveIt(Tags.CAMERABOUNDS))
                Debug.LogWarning(
                    $"Camera bounds {thisGameObject.name} should have the tag {Tags.CAMERABOUNDS}. Instead, it has {thisGameObject.tag}");
        }
#endif
    }
}