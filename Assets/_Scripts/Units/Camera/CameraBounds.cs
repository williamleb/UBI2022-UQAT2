using UnityEngine;

namespace Units.Camera
{
    public class CameraBounds : MonoBehaviour
    {
        [Header("Boundaries")] [SerializeField]
        private Vector3 bounds = Vector3.one;

        [SerializeField] private Vector3 boundsOffset = Vector3.zero;

        [SerializeField] private UnityEngine.Camera mainCamera;

        private bool initialized = false;

        private void Awake() => Initialize();

        private void Initialize()
        {
            bounds.y = 0;

            if (mainCamera == null && UnityEngine.Camera.main != null) mainCamera = UnityEngine.Camera.main;
        }

        private void OnDrawGizmos() => Gizmos.DrawWireCube(boundsOffset, bounds * 2);

        public Vector3 StayWithinBounds(Vector3 position, float cameraTiltAngle, float distance)
        {
            if (!initialized)
                Initialize();

            float horizontalFOV = mainCamera.fieldOfView;
            float verticalFOV =
                2 * Mathf.Atan(Mathf.Tan(horizontalFOV * Mathf.Deg2Rad / 2) * mainCamera.pixelHeight /
                               mainCamera.pixelWidth) * Mathf.Rad2Deg;

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
                                mainCamera.transform.localPosition.z;
            if (position.z > zUpperLimit)
                position.z = zUpperLimit;


            float zLowerLimit = -bounds.z + boundsOffset.z - Mathf.Cos(allowedZAngleLower * Mathf.Deg2Rad) * distance -
                                mainCamera.transform.localPosition.z;
            if (position.z < zLowerLimit)
                position.z = zLowerLimit;

            return position;
        }
    }
}