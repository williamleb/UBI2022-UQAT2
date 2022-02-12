using System;
using BehaviorDesigner.Runtime.Tasks.Unity.UnityGameObject;
using UnityEngine;

namespace Canvases.Markers
{
    [RequireComponent(typeof(RectTransform))]
    public abstract class Marker : MonoBehaviour
    {
        private RectTransform rectTransform;
        private Camera currentCamera;

        private Vector3 worldPosition = Vector3.zero;

        private Action<Marker> release = null;

        public bool IsActivated => release != null;

        public Vector3 Position
        {
            get => worldPosition;
            set => worldPosition = value;
        }

        public float Scale
        {
            get => transform.localScale.x;
            set => transform.localScale = new Vector3(value, value, value);
        }
        
        public void Initialize(Action<Marker> releaseHandle)
        {
            release = releaseHandle;
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
            var screenPosition = currentCamera.WorldToScreenPoint(worldPosition);
            rectTransform.position = screenPosition;
        }
    }
}