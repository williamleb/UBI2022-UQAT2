using System;
using DigitalRuby.Tween;
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

        private bool isOutsideCamera = false;
        private float tweenTargetSize = 0f;
        private FloatTween insideOutsideCameraTransitionTween = null;

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
                UpdateMarkerSize();
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
                UpdateMarkerSize();
                AdjustMarkerScreenPosition();
            }
        }

        private void UpdateMarkerSize()
        {
            if (CameraIsBehind() && !ShowOutsideCameraBorders)
            {
                rectTransform.localScale = new Vector3(0f, 0f, 0f);
                return;
            }
            
            var targetSize = GetMarkerSizeWithCameraDistance();
            var currentSize = rectTransform.localScale.x;

            var newIsOutsideCamera = !IsInsideCamera();
            if (newIsOutsideCamera != isOutsideCamera)
            {
                isOutsideCamera = newIsOutsideCamera;
                
                if (insideOutsideCameraTransitionTween != null)
                {
                    TweenFactory.RemoveTween(insideOutsideCameraTransitionTween, TweenStopBehavior.DoNotModify);
                }

                tweenTargetSize = targetSize;
                var duration = MarkerManager.HasInstance ? MarkerManager.Instance.SecondsOfTransitionsInsideOutsideCamera : 0.1f;

                gameObject.Tween(
                    nameof(insideOutsideCameraTransitionTween), 
                    currentSize, 
                    tweenTargetSize, 
                    duration, 
                    TweenScaleFunctions.QuadraticEaseOut, 
                    t => rectTransform.localScale = t.CurrentValue * Vector3.one);
                return;
            }

            if (insideOutsideCameraTransitionTween != null)
            {
                if (Math.Abs(currentSize - tweenTargetSize) > 0.1f)
                {
                    return;
                }
                
                insideOutsideCameraTransitionTween = null;
            }

            rectTransform.localScale = targetSize * Vector3.one;
        }

        private float GetMarkerSizeWithCameraDistance()
        {
            if (ShowOutsideCameraBorders && !IsInsideCamera())
            {
                return MarkerManager.HasInstance ? MarkerManager.Instance.OutsideCameraScale : 1f;
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

        private void AdjustMarkerScreenPosition()
        {
            var screenPosition = currentCamera.WorldToScreenPoint(worldPosition);
            var halfSize = rectTransform.sizeDelta * transform.lossyScale * 0.5f;

            if (ShowOutsideCameraBorders && CameraIsBehind())
            {
                screenPosition.x = -screenPosition.x;
                screenPosition.y = -screenPosition.y;
            }

            var clampedX = Mathf.Clamp(screenPosition.x, halfSize.x + Padding.x, Screen.width - halfSize.x - Padding.x);
            var clampedY = Mathf.Clamp(screenPosition.y, halfSize.y + Padding.y, Screen.height - halfSize.y - Padding.y);
            
            rectTransform.position = new Vector3(clampedX, clampedY, 0f);
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