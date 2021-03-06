using Cinemachine;
using Fusion;
using Sirenix.OdinInspector;
using Systems.Settings;
using UnityEngine;
using Utilities.Unity;

namespace Units.Camera
{
    public class CameraStrategy : MonoBehaviour
    {
        [Header("Base Settings")] [SerializeField]
        protected float MoveSpeed;

        [SerializeField, Required] private CinemachineVirtualCamera virtualCamera;
        private CinemachineCameraOffset cameraOffset;

        [Header("Boundaries")] [SerializeField]
        private CameraBounds cameraBounds;

        [SerializeField] private Transform target;
        private PlayerSettings.PlayerCameraSettings data;
        private Vector3 offset;
        private Vector3 averageTarget;
        private float longestDistance;

        private bool initialized;

        private void Awake()
        {
            cameraOffset = virtualCamera.GetComponent<CinemachineCameraOffset>();
        }

        public void Init(PlayerSettings.PlayerCameraSettings cameraSettings)
        {
            SetCameraBounds();

            data = cameraSettings;
            initialized = true;
            UpdateCamera();
        }

        public void SetCameraBounds(NetworkRunner _ = null)
        {
            var cameraBoundsObject = GameObject.FindWithTag(Tags.CAMERABOUNDS);
            if (!cameraBoundsObject)
            {
                Debug.LogWarning("No camera bounds found in this scene");
                cameraBounds = gameObject.AddComponent<CameraBounds>();
                cameraBounds.Bounds = new Vector3(50f, 0f, 50f);
            }
            else
            {
                cameraBounds = cameraBoundsObject.GetComponent<CameraBounds>();
            }
        }

        private void UpdateCamera()
        {
            if (!initialized) return;
            CalculateAverages();
            CalculateOffset();

            cameraOffset.m_Offset = offset;
            virtualCamera.transform.rotation = Quaternion.Euler(data.RotX, 0, 0);
            virtualCamera.m_Lens.FieldOfView = data.FieldOfView;

            averageTarget = cameraBounds.StayWithinBounds(virtualCamera, averageTarget, data.RotX, longestDistance);

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