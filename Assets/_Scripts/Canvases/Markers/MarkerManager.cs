using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;
using Utilities.Singleton;

namespace Canvases.Markers
{
    public class MarkerManager : Singleton<MarkerManager>
    {
        [SerializeField, Required] private MarkerManagerData data;

        private Dictionary<Type, List<Marker>> markers = new Dictionary<Type, List<Marker>>();

        /// <summary>
        /// After calling this method to get a marker, you must release the marker when you no longer need it with
        /// the Marker.Release method.
        /// </summary>
        public T InitializeMarker<T>() where T : Marker
        {
            if (!markers.ContainsKey(typeof(T)))
            {
                Debug.LogWarning($"Could not find a marker of type {typeof(T)}");
                return null;
            }
            
            var markerList = markers[typeof(T)];
            foreach (var marker in markerList)
            {
                if (!marker.IsActivated)
                {
                    marker.Initialize(ReleaseMarker);
                    Debug.Log($"Marker of type {marker.GetType()} initialized");
                    return (T) Convert.ChangeType(marker, typeof(T));;
                }
            }

            return null;
        }

        private void ReleaseMarker(Marker marker)
        {
            Debug.Log($"Marker of type {marker.GetType()} released");
        }

        private void Start()
        {
            InstantiateMarkers();
        }

        private void InstantiateMarkers()
        {
            foreach (var markerType in data)
            {
                var markerData = data[markerType];
                InstantiateTypeMarkers(markerData);
            }
        }

        private void InstantiateTypeMarkers(MarkerData markerData)
        {
            markers[markerData.MarkerType] = new List<Marker>();
            for (var i = 0; i < markerData.NumberOfInstances; ++i)
            {
                var markerGameObject = Instantiate(markerData.Prefab, transform);
                markerGameObject.name = markerData.MarkerType.ToString().Split('.').Last();
                
                var marker = markerGameObject.GetComponent<Marker>();
                Debug.Assert(marker, "Marker prefabs should have a marker");
                Debug.Assert(marker.GetType().IsAssignableFrom(markerData.MarkerType), "Marker prefabs should have a marker of correct type");
                
                markers[markerData.MarkerType].Add(marker);
            }
        }
    }
}