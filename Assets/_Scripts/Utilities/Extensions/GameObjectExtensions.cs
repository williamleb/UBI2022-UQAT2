using Fusion;
using UnityEngine;
using Utilities.Unity;
using Behaviour = Fusion.Behaviour;

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
        public static T GetComponentInEntity<T>(this GameObject gameObject) where T : Component
        {
            var component = gameObject.GetComponentInParent<T>();
            if (!component)
            {
                component = gameObject.GetComponent<T>();
            }
            if (!component)
            {
                component = gameObject.GetComponentInChildren<T>();
            }
            if (!component)
            {
                component = gameObject.transform.parent.GetComponentInChildren<T>();
            }

            return component;
        }
        
        public static T GetComponentInEntity<T>(this NetworkObject gameObject) where T : Component
        {
            return gameObject.gameObject.GetComponentInEntity<T>();
        }
        
        public static GameObject GetParent(this GameObject gameObject)
        {
            var parent = gameObject.transform.parent;
            return parent ? parent.gameObject : gameObject;
        }

        public static bool CompareEntities(this GameObject gameObject, GameObject other)
        {
            return other == gameObject || 
                   other.GetParent() == gameObject ||
                   other.GetParent() == gameObject.GetParent() || 
                   other == gameObject.GetParent();
        }
        
        public static bool HasTag(this GameObject gameObject)
        {
            return !gameObject.CompareTag(Tags.UNTAGGED);
        }
        
        public static bool AssignTagIfDoesNotHaveIt(this GameObject gameObject, string tag)
        {
            if (!gameObject.HasTag())
                gameObject.tag = tag;

            return gameObject.CompareTag(tag);
        }
        
        public static bool HasLayer(this GameObject gameObject)
        {
            return gameObject.layer != Layers.DEFAULT;
        }
        
        public static bool AssignLayerIfDoesNotHaveIt(this GameObject gameObject, int layer)
        {
            if (!gameObject.HasLayer())
                gameObject.layer = layer;

            return gameObject.layer == layer;
        }
        
        public static bool IsAnAI(this GameObject gameObject)
        {
            return gameObject.CompareTag(Tags.AI) || gameObject.GetParent().CompareTag(Tags.AI);
        }
        
        public static bool IsAPlayer(this GameObject gameObject)
        {
            return gameObject.CompareTag(Tags.PLAYER) || gameObject.GetParent().CompareTag(Tags.PLAYER);
        }
    }
}