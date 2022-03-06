using System;
using Fusion;
using Units.Player;
using UnityEngine;
using Utilities.Extensions;

namespace Canvases.Markers
{
    [RequireComponent(typeof(RectTransform))]
    public abstract class Marker : MonoBehaviour
    {
        private RectTransform rectTransform;
        private Camera currentCamera;

        private Vector3 worldPosition = Vector3.zero;
        private float scale = 1.0f;

        private Action<Marker> release;

        public bool IsActivated => release != null;

        public Vector3 Position
        {
            get => worldPosition;
            set => worldPosition = value;
        }

        public bool ShowOutsideCameraBorders { get; set; } = true; // TODO
        public Vector2 Padding { get; set; } = new Vector2(10f, 10f); // TODO

        public float Scale
        {
            get => scale;
            set => scale = value;
        }

        public void Initialize(Action<Marker> releaseHandle)
        {
            release = releaseHandle;
        }

        public void Activate()
        {
            if (currentCamera != null)
            {
                AdjustMarkerSizeWithCameraDistance();
                AdjustMarkerScreenPosition();
            }
            gameObject.SetActive(true);
        }

        public void Release()
        {
            if (release == null)
                return;

            release.Invoke(this);
            release = null;
            gameObject.SetActive(false);
        }

        protected virtual void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
            PlayerEntity.OnPlayerSpawned += Init;
        }

        private void Start()
        {
            gameObject.SetActive(false);
        }

        protected virtual void Init(NetworkObject networkObject)
        {
            if (networkObject.HasInputAuthority)
            {
                // This might have to be replaced if we ever decide to switch camera in the middle of the game
                currentCamera = networkObject.GetComponentInEntity<Camera>();
                Debug.Assert(currentCamera != null, $"The script {nameof(Marker)} needs a {nameof(Camera)} in the scene");
            }
        }

        protected virtual void Update()
        {
            if (currentCamera != null)
            {
                AdjustMarkerSizeWithCameraDistance();
                AdjustMarkerScreenPosition();
            }
        }

        private void AdjustMarkerSizeWithCameraDistance()
        {
            if (CameraIsBehind() && !ShowOutsideCameraBorders)
            {
                rectTransform.localScale = new Vector3(0f, 0f, 0f);
                return;
            }

            var cameraDistance = Vector3.Distance(currentCamera.transform.position, worldPosition);
            var generalScale = MarkerManager.HasInstance ? scale * MarkerManager.Instance.GeneralScale : scale;
            var adjustedScale = Math.Abs(cameraDistance) > 0.001f ? generalScale / cameraDistance : generalScale;
            rectTransform.localScale = new Vector3(adjustedScale, adjustedScale, adjustedScale);
        }

        private bool CameraIsBehind()
        {
            var cameraTransform = currentCamera.transform;
            var cameraToPosition = worldPosition - cameraTransform.position;
            return Vector3.Dot(cameraToPosition, cameraTransform.forward) < 0;
        }

        private void AdjustMarkerScreenPosition()
        {
            var screenPosition = currentCamera.WorldToScreenPoint(worldPosition);
            var halfSize = rectTransform.sizeDelta * transform.lossyScale * 0.5f;

            Debug.Log($"Screen position: {screenPosition}");
            Debug.Log($"Screen dimensions: {new Vector2(Screen.width, Screen.height)}");
            Debug.Log($"Size: {halfSize * 2f}");

            
            if (CameraIsBehind())
            {
                var cameraToPositionDirection = (Position - currentCamera.transform.position).normalized;
                var newX = Vector3.Dot(currentCamera.transform.right, cameraToPositionDirection);
                var newY = Vector3.Dot(currentCamera.transform.up, cameraToPositionDirection);

                screenPosition = new Vector3(newX * Screen.width, newY * Screen.height, 0f);

                Debug.Log($"The vector position: {screenPosition}");
            }

            var x = Mathf.Clamp(screenPosition.x, halfSize.x + Padding.x, Screen.width - halfSize.x - Padding.x);
            var y = Mathf.Clamp(screenPosition.y, halfSize.y + Padding.y, Screen.height - halfSize.y - Padding.y);
            screenPosition = new Vector3(x, y, 0f);
            
            Debug.Log($"Clamped: {screenPosition}");
            
            rectTransform.position = screenPosition;
        }
    }
}