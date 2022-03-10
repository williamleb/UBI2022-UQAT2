using Systems.Settings;
using UnityEngine;

namespace Units.Camera
{
    public class CameraStrategy : MonoBehaviour
    {
        [Header("Base Settings")] [SerializeField]
        protected float MoveSpeed;

        [SerializeField] protected UnityEngine.Camera MyCamera;

        [Header("Boundaries")] [SerializeField]
        private CameraBounds cameraBounds;

        private Transform target;
        private PlayerSettings.PlayerCameraSettings data;
        private Vector3 offset;
        private Vector3 averageTarget;
        private float longestDistance;

        private bool initialized;

        private void Awake()
        {
            if (MyCamera == null && UnityEngine.Camera.main != null) MyCamera = UnityEngine.Camera.main;
        }

        public void Init(Transform targetTransform, PlayerSettings.PlayerCameraSettings cameraSettings)
        {
            target = targetTransform;
            data = cameraSettings;
            initialized = true;
            UpdateCamera();
        }

        private void UpdateCamera()
        {
            if (!initialized) return;
            CalculateAverages();
            CalculateOffset();

            MyCamera.transform.localPosition = offset;
            MyCamera.transform.rotation = Quaternion.Euler(data.RotX, 0, 0);
            MyCamera.fieldOfView = data.FieldOfView;

            averageTarget = cameraBounds.StayWithinBounds(averageTarget, data.RotX, longestDistance);

            UpdatePositionAndRotation();
        }

        private void LateUpdate() => UpdateCamera();

        private void CalculateAverages()
        {
            averageTarget = target.position;
            longestDistance = Mathf.Sqrt(data.PosY * data.PosY + data.PosZ * data.PosZ);
        }

        private void CalculateOffset() => offset.Set(data.PosX, data.PosY, data.PosZ);

        private void UpdatePositionAndRotation() => transform.position =
            Vector3.Lerp(transform.position, averageTarget, Time.fixedDeltaTime * MoveSpeed);
    }
}