using System.Collections.Generic;
using UnityEngine;

namespace Units.Camera
{
    public class CameraStrategy : MonoBehaviour
    {
        [Header("Base Settings")] [SerializeField]
        protected float MoveSpeed;
        
        [SerializeField] protected Transform MyCamera;

        [Header("-- Targets")] protected readonly List<GameObject> Targets = new List<GameObject>();
        protected readonly List<GameObject> ActiveTargets = new List<GameObject>();

        protected virtual void Initialize()
        {
            if (MyCamera == null && UnityEngine.Camera.main != null) MyCamera = UnityEngine.Camera.main.transform;
        }

        public void AddTarget(GameObject target)
        {
            Targets.Add(target);
        }

        public void RemoveTarget(GameObject target)
        {
            Targets.Remove(target);
        }

        public virtual void ClearTargets()
        {
            Targets.Clear();
            ActiveTargets.Clear();
        }

        public void RemoveAll()
        {
            Targets.Clear();
        }
    }
}