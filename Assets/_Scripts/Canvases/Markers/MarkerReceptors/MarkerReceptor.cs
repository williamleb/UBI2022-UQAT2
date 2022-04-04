using System;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Canvases.Markers
{
    public abstract class MarkerReceptor<T> : MonoBehaviour where T : Marker
    {
        [SerializeField] private float markerScale = 1.0f;
        [SerializeField] private bool lockInPlace = true;
        [SerializeField] private bool showOutsideCameraBorders;
        [SerializeField] private bool activateOnStart;
        [SerializeField, ShowIf(nameof(showOutsideCameraBorders))] private Vector2 padding = new Vector2(10f, 10f);
        [SerializeField, Tooltip("Will set this marker on top of others. Use sparingly.")] private bool important;

        [SerializeField] private bool animate;
        [SerializeField, ShowIf(nameof(animate))] private AnimationCurve animationCurve = new AnimationCurve();
        
        private T currentMarker;
        private Vector3 offsetToParent;
        
        private float animationTime;

        [ShowInInspector] public bool IsActivated => currentMarker != null;

        public T CurrentMarker => currentMarker;

        private void Awake()
        {
            var thisTransform = transform;
            offsetToParent = thisTransform.position - thisTransform.parent.position;
        }

        private void Start()
        {
            if (activateOnStart)
                Activate();
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
                Debug.LogWarning($"Could not activate the marker receptor because there was no marker of type {typeof(T)} left. Please add more instances of this marker in this marker data.");
                return;
            }

            animationTime = 0f;
            
            currentMarker = marker;
            currentMarker.DefaultScale = currentMarker.Scale = markerScale;
            currentMarker.Position = transform.position;
            currentMarker.ShowOutsideCameraBorders = showOutsideCameraBorders;
            currentMarker.Padding = padding;
            currentMarker.Activate();
            if(important) currentMarker.transform.SetAsLastSibling();
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

            if (animate && !currentMarker.IsTransitioning)
            {
                Animate();
            }
        }

        private void Animate()
        {
            var value = animationCurve.Evaluate(animationTime);
            currentMarker.Scale = currentMarker.DefaultScale + value;
            animationTime += Time.deltaTime;
            
            if (animationTime > animationCurve.keys.Last().time)
            {
                animationTime = 0f;
            }
        }
    }
}