using System;
using UnityEngine;
using Utilities.Extensions;

namespace Utilities.Singleton
{
    public abstract class AbstractSingleton<T> : MonoBehaviour where T : AbstractSingleton<T>
    {
        protected static T SharedInstance;

        public static bool HasInstance => SharedInstance != null;

        protected virtual void Awake()
        {
            try
            {
                SharedInstance = (T) Convert.ChangeType(this, typeof(T));
            }
            catch (InvalidCastException)
            {
                Debug.Assert(false, "Singleton's T type should be the derived class.");
            }
        }

        protected void DestroyInstance()
        {
            if (gameObject.GetNumberOfComponents() == 1)
            {
                Destroy(gameObject);
            }
            else
            {
                Destroy(this);
            }
        }

        protected virtual void OnApplicationQuit()
        {
            SharedInstance = null;
            DestroyInstance();
        }
    }

    /// <summary>
    /// Singleton class that can be loaded and unloaded with scenes.
    /// </summary>
    public abstract class Singleton<T> : AbstractSingleton<T> where T : Singleton<T>
    {
        public static T Instance
        {
            get
            {
                Debug.Assert(HasInstance, $"Trying to access a script of type {typeof(T).Name} that is not in the scene.");

                return SharedInstance;
            }
        }

        protected override void Awake()
        {
            if (HasInstance)
            {
                Debug.LogWarning($"New instance of type {typeof(T).Name} detected. " +
                                 "This new instance is becoming the default instance.");
                return;
            }
            
            base.Awake();
        }

        protected virtual void OnDestroy()
        {
            if (SharedInstance == this)
            {
                SharedInstance = null;
            }
        }
    }

    /// <summary>
    /// Singleton class that is lazy loaded once and is only unloaded when the game exits.
    /// </summary>
    public abstract class PersistentSingleton<T> : AbstractSingleton<T> where T : PersistentSingleton<T>
    {
        public static T Instance
        {
            get
            {
                if (!HasInstance)
                {
                    CreateInstance();
                }

                return SharedInstance;
            }
        }

        private static void CreateInstance()
        {
            var instanceGameObject = new GameObject(typeof(T).Name);
            instanceGameObject.AddComponent<T>();
        }
        
        protected override void Awake()
        {
            if (HasInstance)
            {
                Debug.Log($"Two or more instances of a singleton of type {typeof(T).Name} were found in the scene. " +
                          "The new instance of the singleton trying to register will be removed.");

                DestroyInstance();
                return;
            }
            
            DontDestroyOnLoad(gameObject);
            base.Awake();
        }
    }

}