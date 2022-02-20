using System;
using UnityEngine;

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
            AdjustMarkerSizeWithCameraDistance();
            AdjustMarkerScreenPosition();
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
        }

        protected virtual void Start()
        {
            // This might have to be replaced if we ever decide to switch camera in the middle of the game
            currentCamera = Camera.main;
            Debug.Assert(currentCamera, $"The script {nameof(Marker)} needs a {nameof(Camera)} in the scene");
            
            gameObject.SetActive(false);
        }

        protected virtual void Update()
        {
            AdjustMarkerSizeWithCameraDistance();
            AdjustMarkerScreenPosition();
        }

        private void AdjustMarkerSizeWithCameraDistance()
        {
            if (CameraIsBehind())
            {
                rectTransform.localScale = new Vector3(0f, 0f, 0f);
                return;
            }
            
            var cameraDistance = Vector3.Distance(currentCamera.transform.position, worldPosition);
            var adjustedScale = Math.Abs(cameraDistance) > 0.001f ? scale / cameraDistance : scale;
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
            rectTransform.position = screenPosition;
        }
    }
}