using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Utilities
{
    public static class DontDestroyOnLoadUtils
    {
        private static List<GameObject> managedObjects = new List<GameObject>();

        public static void Add(GameObject gameObject)
        {
            managedObjects.Add(gameObject);
            Object.DontDestroyOnLoad(gameObject);
        }

        public static void DestroyAll(Func<GameObject, bool> filter = null)
        {
            foreach (var managedObject in managedObjects)
            {
                if (filter != null && !filter.Invoke(managedObject))
                    continue;
                
                Object.DestroyImmediate(managedObject);
            }
            managedObjects.Clear();
        }
    }
}