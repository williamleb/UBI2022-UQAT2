using Canvases.Markers;
using UnityEngine;

namespace Ingredients.Volumes
{
    [RequireComponent(typeof(LocalPlayerDetection))]
    public abstract class LocalPlayerVolume : MonoBehaviour
    {
        private LocalPlayerDetection detection;

        protected LocalPlayerDetection Detection => detection;

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
            ActivateImplementation();
        }

        private void Deactivate()
        {
            DeactivateImplementation();
        }

        protected abstract void ActivateImplementation();
        protected abstract void DeactivateImplementation();
    }
}