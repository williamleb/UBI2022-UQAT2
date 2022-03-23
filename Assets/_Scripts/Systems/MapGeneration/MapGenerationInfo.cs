using System;
using System.Collections.Generic;
using Fusion;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Systems.MapGeneration
{
    public class MapGenerationInfo : MonoBehaviour
    {
        public IEnumerable<RoomGenerationInfo> Rooms => rooms;
        public Transform PropParent => propParent;

        [SerializeField] private Transform propParent;
        [SerializeField] private RoomGenerationInfo[] rooms;

        [Button(ButtonSizes.Large)]
        private void FindRoomsInPrefab()
        {
            rooms = GetComponentsInChildren<RoomGenerationInfo>();
        }

        [Button(ButtonSizes.Large)]
        private void FindPropParent()
        {
            if (propParent != null) return;
            GameObject pp = GameObject.Find("Props");
            if (pp)
            {
                if (!pp.TryGetComponent(out NetworkObject obj))
                {
                    pp.AddComponent<NetworkObject>();
                }
                propParent = pp.transform;
            }
            else
            {
                Debug.LogWarning("Need to create a 'Props' gameObject to hold all props");
            }
        }

        private void OnValidate()
        {
            FindRoomsInPrefab();
            FindPropParent();
        }
    }
}