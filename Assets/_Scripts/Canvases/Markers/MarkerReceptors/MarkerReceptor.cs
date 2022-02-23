using System;
using UnityEngine;

namespace Canvases.Markers
{
    public abstract class MarkerReceptor<T> : MonoBehaviour where T : Marker
    {
        [SerializeField] private float markerScale = 1.0f;
        [SerializeField] private bool lockInPlace = true;
        
        private T currentMarker;
        private Vector3 offsetToParent;
        
        public bool IsActivated => currentMarker != null;

        public T CurrentMarker => currentMarker;

        private void Awake()
        {
            var thisTransform = transform;
            offsetToParent = thisTransform.position - thisTransform.parent.position;
        }

        public void Activate()
        {
            if (IsActivated)
                return;
            
            if (!MarkerManager.HasInstance)
            {
                Debug.LogWarning($"Could not activate the marker receptor because no {nameof(MarkerManager)} was found in the scene.");
                return;
            }

            var marker = MarkerManager.Instance.InitializeMarker<T>();
            if (marker == null)
            {
                Debug.LogWarning($"Could not activate the marker receptor because there was no marker of type {typeof(T)} left.");
                return;
            }


            currentMarker = marker;
            currentMarker.Scale = markerScale;
            currentMarker.Position = transform.position;
            currentMarker.Activate();
            OnActivated();
        }

        public void Deactivate()
        {
            if (!IsActivated)
                return;
            
            currentMarker.Release();
            currentMarker = null;
            OnDeactivated();
        }

        protected virtual void OnActivated() { }
        protected virtual void OnDeactivated() { }

        protected virtual void Update()
        {
            if (!IsActivated)
                return;

            var thisTransform = transform;
            if (lockInPlace)
            {
                thisTransform.position = thisTransform.parent.position + offsetToParent;
            }
            
            currentMarker.Position = thisTransform.position;
        }
    }
}