using UnityEngine;

namespace Units.Camera
{
    public class CameraBounds : MonoBehaviour
	{
		[Header("Boundaries")] [SerializeField] private Vector3 bounds = Vector3.one;

		[SerializeField] private Vector3 boundsOffset = Vector3.zero;
		[SerializeField] private float boundsMovementMax = 2f;
		private float boundsMovementMultiplier;

		[SerializeField] private UnityEngine.Camera mainCamera;

		private float boundsDiagonal;

		public Vector3 Bounds => bounds;

		public float Diagonal => boundsDiagonal;

		public float SetBoundsMovementMultiplier
		{
			set => boundsMovementMultiplier = value;
		}


		private bool initialized = false;

		// Start is called before the first frame update
		private void Awake()
		{
			Initialize();
		}

		private void Initialize()
		{
			bounds.y = 0;
			boundsDiagonal = Vector3.Distance(bounds, -bounds) * 2;

			if (mainCamera == null && UnityEngine.Camera.main != null) mainCamera = UnityEngine.Camera.main;
		}

		private void OnDrawGizmos()
		{
			Vector3 localBoundsOffset = boundsOffset + (new Vector3(0, 0, boundsMovementMax * boundsMovementMultiplier));
			Gizmos.DrawWireCube(localBoundsOffset, bounds * 2);
		}

		public Vector3 StayWithinBounds(Vector3 position, float cameraTiltAngle, float distance, Transform cameraTrans)
		{
			if (!initialized)
				Initialize();

			Vector3 localBoundsOffset = boundsOffset + (new Vector3(0, 0, boundsMovementMax * boundsMovementMultiplier));

			float horizontalFOV = mainCamera.fieldOfView;
			float verticalFOV = 2 * Mathf.Atan(Mathf.Tan((horizontalFOV * Mathf.Deg2Rad) / 2) * mainCamera.pixelHeight / mainCamera.pixelWidth) * Mathf.Rad2Deg;

			float allowedXAngle = 90 - (horizontalFOV / 2);
			float allowedZAngleLower = cameraTiltAngle + (verticalFOV / 2);
			float allowedZAngleUpper = cameraTiltAngle - (verticalFOV / 2);

			//Limit the x-left movement
			float xLeftLimit = (-Bounds.x + localBoundsOffset.x) + Mathf.Cos(allowedXAngle * Mathf.Deg2Rad) * distance;
			if (position.x < xLeftLimit)
				position.x = xLeftLimit;

			//Limit the x-right movement
			float xRightLimit = (Bounds.x + localBoundsOffset.x) - Mathf.Cos(allowedXAngle * Mathf.Deg2Rad) * distance;
			if (position.x > xRightLimit)
				position.x = xRightLimit;

			float zUpperLimit = (Bounds.z + localBoundsOffset.z) - Mathf.Cos(allowedZAngleUpper * Mathf.Deg2Rad) * distance - cameraTrans.localPosition.z;
			if (position.z > zUpperLimit)
				position.z = zUpperLimit;


			float zLowerLimit = (-Bounds.z + localBoundsOffset.z) - Mathf.Cos(allowedZAngleLower * Mathf.Deg2Rad) * distance - cameraTrans.localPosition.z;
			if (position.z < zLowerLimit)
				position.z = zLowerLimit;

			boundsMovementMultiplier = 0;
			return position;
		}
	}
}