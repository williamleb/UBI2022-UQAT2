using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine;

namespace Canvases.Markers
{
    [CreateAssetMenu(menuName = "Canvases/MarkerManagerData")]
    public class MarkerManagerData : SerializedScriptableObject
    {
        private const string MARKERS_FOLDER_PATH = "Canvases/Markers";
        
        [OdinSerialize] [ReadOnly] [BehaviorDesigner.Runtime.Tasks.Tooltip("This dictionary only accepts types that derive from the Marker class")]
        private Dictionary<Type, MarkerData> markerData = new Dictionary<Type, MarkerData>();

        public MarkerData GetMarkerData<T>()
        {
            var markerType = typeof(T);
            if (!markerData.ContainsKey(markerType))
                return null;

            return markerData[markerType];
        }

        [Button]
        private void FetchMarkerData()
        {
            LoadMarkersData();
            RemoveInvalidEntries();
        }

        private void LoadMarkersData()
        {
            markerData.Clear();
            
            var markerDataList = Resources.LoadAll<MarkerData>(MARKERS_FOLDER_PATH);
            Debug.Assert(markerDataList.Any(), $"An object of type {nameof(MarkerData)} should be in the folder {MARKERS_FOLDER_PATH}");

            foreach (var markerDataElement in markerDataList)
            {
                markerData.Add(markerDataElement.MarkerType, markerDataElement);
            }
        }
        
        private void OnValidate()
        {
            RemoveInvalidEntries();
        }

        private void RemoveInvalidEntries()
        {
            var invalidEntries = markerData.Keys.Where(markerType => !markerType.IsSubclassOf(typeof(Marker))).ToList();
            foreach (var invalidEntry in invalidEntries)
            {
                markerData.Remove(invalidEntry);
            }
        }
    }
}