using System;
using DigitalRuby.Tween;
using Systems.Camera;
using UnityEngine;

namespace Canvases.Markers
{
    [RequireComponent(typeof(RectTransform))]
    public abstract class Marker : MonoBehaviour
    {
        private const float SMALLEST_SCALE = 0.1f;
        
        private RectTransform rectTransform;
        private Camera currentCamera;

        private Vector3 worldPosition = Vector3.zero;
        private float defaultScale = 1.0f;
        private float scale = 1.0f;

        private Action<Marker> release;

        private bool justActivated;
        private bool isOutsideCamera;
        private bool showOutsideCameraBorders;
        private FloatTween insideOutsideCameraTransitionTween;

        public bool IsActivated => release != null;
        public bool IsTransitioning => insideOutsideCameraTransitionTween != null;

        public Vector3 Position
        {
            get => worldPosition;
            set => worldPosition = value;
        }
        
        public bool ShowOutsideCameraBorders
        {
            get => showOutsideCameraBorders && (!MarkerManager.HasInstance || !MarkerManager.Instance.HideMarkersOutsideView);
            set => showOutsideCameraBorders = value;
        }
        
        public Vector2 Padding { get; set; } = new Vector2(10f, 10f);

        public float DefaultScale
        {
            get => defaultScale;
            set => defaultScale = value;
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
            justActivated = true;
            RemoveCurrentTween();
            isOutsideCamera = false;
            
            if (currentCamera != null)
            {
                rectTransform.localScale = SMALLEST_SCALE * Vector3.one;
                UpdateMarkerScreenPosition();
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
        }

        private void Start()
        {
            currentCamera = CameraSystem.Instance.MainCamera;
            
            if (!IsActivated)
                gameObject.SetActive(false);
        }

        protected virtual void Update()
        {
            if (currentCamera != null)
            {
                UpdateMarkerSize();
                UpdateMarkerScreenPosition();
            }
        }

        private void HideMarker()
        {
            rectTransform.localScale = new Vector3(0f, 0f, 0f);
        }

        private void UpdateMarkerSize()
        {
            if (!ShowOutsideCameraBorders && CameraIsBehind())
            {
                HideMarker();
                return;
            }

            if (!ManageFinishedTween())
                return;
            
            var targetSize = GetMarkerSizeWithCameraDistance(); 
            if (justActivated)
            {
                justActivated = false;
                StartTween(SMALLEST_SCALE, targetSize);
                return;
            }
            
            if (ShowOutsideCameraBorders && UpdateInsideOutsideCameraTween(targetSize))
            {
                return;
            }

            rectTransform.localScale = targetSize * Vector3.one;
        }

        private bool UpdateInsideOutsideCameraTween(float targetSize)
        {
            var currentSize = rectTransform.localScale.x;

            var newIsOutsideCamera = !IsInsideCamera();
            if (newIsOutsideCamera != isOutsideCamera)
            {
                isOutsideCamera = newIsOutsideCamera;
                StartTween(currentSize, targetSize);
                return true;
            }

            return false;
        }

        private bool ManageFinishedTween()
        {
            if (insideOutsideCameraTransitionTween != null)
            {
                if (insideOutsideCameraTransitionTween.State != TweenState.Stopped)
                    return false;
                
                insideOutsideCameraTransitionTween = null;
            }

            return true;
        }

        private void StartTween(float currentSize, float targetSize)
        {
            RemoveCurrentTween();
            
            var duration = MarkerManager.HasInstance ? MarkerManager.Instance.SecondsOfTransitionsInsideOutsideCamera : 0.1f;

            insideOutsideCameraTransitionTween = gameObject.Tween(
                $"MarkerScale-{GetInstanceID()}", 
                currentSize, 
                targetSize, 
                duration, 
                TweenScaleFunctions.QuadraticEaseOut, 
                t =>
                {
                    if (rectTransform)
                        rectTransform.localScale = t.CurrentValue * Vector3.one;
                });
        }

        private void RemoveCurrentTween()
        {
            if (insideOutsideCameraTransitionTween != null)
            {
                TweenFactory.RemoveTween(insideOutsideCameraTransitionTween, TweenStopBehavior.DoNotModify);
                insideOutsideCameraTransitionTween = null;
            }
        }

        private float GetMarkerSizeWithCameraDistance()
        {
            if (ShowOutsideCameraBorders && !IsInsideCamera())
            {
                return MarkerManager.HasInstance ? MarkerManager.Instance.OutsideCameraScale * (scale / defaultScale) : 1f;
            }
            
            var cameraDistance = Vector3.Distance(currentCamera.transform.position, worldPosition);
            var generalScale = MarkerManager.HasInstance ? scale * MarkerManager.Instance.GeneralScale : scale;
            return Math.Abs(cameraDistance) > 0.001f ? generalScale / cameraDistance : generalScale;
        }

        private bool CameraIsBehind()
        {
            var cameraTransform = currentCamera.transform;
            var cameraToPosition = worldPosition - cameraTransform.position;
            return Vector3.Dot(cameraToPosition, cameraTransform.forward) < 0;
        }

        private void UpdateMarkerScreenPosition()
        {
            var screenPosition = currentCamera.WorldToScreenPoint(worldPosition);

            rectTransform.position = ShowOutsideCameraBorders switch
            {
                true => GetScreenClampedPosition(screenPosition),
                false => screenPosition
            };
        }

        private Vector3 GetScreenClampedPosition(Vector3 screenPosition)
        {
            var halfSize = rectTransform.sizeDelta * transform.lossyScale * 0.5f;

            if (CameraIsBehind())
            {
                screenPosition.x = -screenPosition.x;
                screenPosition.y = -screenPosition.y;
            }
                
            var clampedX = Mathf.Clamp(screenPosition.x, halfSize.x + Padding.x, Screen.width - halfSize.x - Padding.x);
            var clampedY = Mathf.Clamp(screenPosition.y, halfSize.y + Padding.y, Screen.height - halfSize.y - Padding.y);
            
            return new Vector3(clampedX, clampedY, 0f);
        }

        private bool IsInsideCamera()
        {
            if (CameraIsBehind())
                return false;
            
            var screenPosition = currentCamera.WorldToScreenPoint(worldPosition);
            
            return screenPosition.x > Padding.x && screenPosition.x < Screen.width - Padding.x &&
                   screenPosition.y > Padding.y && screenPosition.y < Screen.height - Padding.y;
        }
    }
}