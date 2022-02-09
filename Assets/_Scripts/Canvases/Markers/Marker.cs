using System;
using UnityEngine;

namespace Canvases.Markers
{
    [RequireComponent(typeof(RectTransform))]
    public class Marker : MonoBehaviour
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