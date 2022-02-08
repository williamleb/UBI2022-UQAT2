 using UnityEngine;

 namespace Utilities.Extensions
{
    public static class GameObjectExtensions
    {
        public static int GetNumberOfComponents(this GameObject gameObject) 
            => gameObject.GetComponentsInChildren<Component>().Length - 1;

        public static void Show(this GameObject gameObject) => gameObject.SetActive(true);

        public static void Hide(this GameObject gameObject) => gameObject.SetActive(false);
    }
}