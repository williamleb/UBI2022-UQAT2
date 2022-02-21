using System;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine;

namespace Canvases.Markers
{

    [CreateAssetMenu(menuName = "Canvases/MarkerData")]
    public class MarkerData : SerializedScriptableObject
    {
        [OdinSerialize, Required, ValidateInput(nameof(ValidateType), "Type must be a subclass of Marker")] 
        private Type markerType;
        [OdinSerialize, Required] 
        private GameObject markerPrefab;
        [OdinSerialize] 
        private int numberOfInstances;

        public Type MarkerType => markerType;
        public GameObject Prefab => markerPrefab;
        public int NumberOfInstances => numberOfInstances;

        private void OnValidate()
        {
            numberOfInstances = Math.Max(1, numberOfInstances);
        }

        private bool ValidateType()
        {
            return markerType.IsSubclassOf(typeof(Marker));
        }
    }
}