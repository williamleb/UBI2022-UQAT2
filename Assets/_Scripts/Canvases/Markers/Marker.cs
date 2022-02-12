using System;
using UnityEngine;

namespace Canvases.Markers
{
    [RequireComponent(typeof(RectTransform))]
    public abstract class Marker : MonoBehaviour
    {
        private RectTransform rectTransform;
        private Camera camera;

        private Vector3 worldPosition = Vector3.zero;

        public Vector3 Position
        {
            get => worldPosition;
            set => worldPosition = value;
        }

        private void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
        }

        private void Start()
        {
            // This might have to be replaced if we ever decide to switch camera in the middle of the game
            camera = Camera.main;
            Debug.Assert(camera, $"The script {nameof(Marker)} needs a {nameof(Camera)} in the scene");
        }

        private void Update()
        {
            var screenPosition = camera.WorldToScreenPoint(worldPosition);
            rectTransform.position = screenPosition;
        }
    }
}