using Fusion;
using UnityEngine;

 namespace Utilities.Extensions
{
    public static class GameObjectExtensions
    {
        public static int GetNumberOfComponents(this GameObject gameObject) 
            => gameObject.GetComponentsInChildren<Component>().Length - 1;

        public static void Show(this GameObject gameObject) => gameObject.SetActive(true);

        public static void Hide(this GameObject gameObject) => gameObject.SetActive(false);

        public static bool IsVisible(this GameObject gameObject) => gameObject.activeSelf;

        /// <summary>
        /// First, look in the parent if it has the component, then the current gameobject
        /// This is done because entities have their collider a level lower than their parent game object with contains
        /// most of their scripts.
        /// </summary>
        public static T GetComponentInEntity<T>(this GameObject gameObject) where T : MonoBehaviour
        {
            var component = gameObject.GetComponentInParent<T>();
            if (!component)
            {
                component = gameObject.GetComponent<T>();
            }

            return component;
        }
        
        public static T GetComponentInEntity<T>(this NetworkObject gameObject) where T : MonoBehaviour
        {
            return gameObject.gameObject.GetComponentInEntity<T>();
        }
        
        public static GameObject GetParent(this GameObject gameObject)
        {
            var parent = gameObject.transform.parent;
            return parent ? parent.gameObject : gameObject;
        }
    }
}