using UnityEngine;

namespace Ingredients.Volumes.WorldObjects
{
    [RequireComponent(typeof(WorldObjectsDetection))]
    public class WorldBorderVolume : MonoBehaviour
    {
        private WorldObjectsDetection detection;

        private void Awake()
        {
            detection = GetComponent<WorldObjectsDetection>();
        }

        private void Start()
        {
            detection.OnWorldObjectLeft += NotifyObjectEscapedWorld;
        }

        private void OnDestroy()
        {
            detection.OnWorldObjectLeft -= NotifyObjectEscapedWorld;
        }

        private void NotifyObjectEscapedWorld(IWorldObject worldObject)
        {
            worldObject.OnEscapedWorld();
        }
    }
}