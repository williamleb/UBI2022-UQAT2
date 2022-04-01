using System;
using Canvases.Markers;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Ingredients.Volumes
{
    [RequireComponent(typeof(LocalPlayerDetection))]
    public class TextVolume : MonoBehaviour
    {
        [SerializeField] private TextMarkerReceptor textMarker;

        private LocalPlayerDetection detection;

        private void Awake()
        {
            detection = GetComponent<LocalPlayerDetection>();
        }

        private void Start()
        {
            detection.OnLocalPlayerEntered += Activate;
            detection.OnLocalPlayerLeft += Deactivate;
        }

        private void OnDestroy()
        {
            detection.OnLocalPlayerEntered -= Activate;
            detection.OnLocalPlayerLeft -= Deactivate;
        }

        private void Activate()
        {
            if (textMarker) textMarker.Activate();
        }

        private void Deactivate()
        {
            if (textMarker) textMarker.Deactivate();
        }
    }
}