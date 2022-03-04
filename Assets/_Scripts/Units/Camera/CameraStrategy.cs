using System.Collections.Generic;
using UnityEngine;

namespace Units.Camera
{
    public class CameraStrategy : MonoBehaviour
    {
        [Header("Base Settings")] [SerializeField]
        protected float MoveSpeed;

        private float initialMoveSpeed;
        protected Transform MyCamera;

        [Header("-- Targets")] protected readonly List<GameObject> Targets = new List<GameObject>();
        protected readonly List<GameObject> ActiveTargets = new List<GameObject>();

        private bool usingPlaceholderTarget;
        private GameObject placeholderTarget;

        private bool setPlaceholder = true;

        protected virtual void Initialize()
        {
            if (UnityEngine.Camera.main != null) MyCamera = UnityEngine.Camera.main.transform;

            initialMoveSpeed = MoveSpeed;
        }

        public void AddTarget(GameObject target)
        {
            if (usingPlaceholderTarget)
            {
                Targets.Clear();
                usingPlaceholderTarget = false;
            }

            Targets.Add(target);
        }

        public void RemoveTarget(GameObject target)
        {
            Targets.Remove(target);
            if (Targets.Count == 0)
                UsePlaceholderTarget();
        }

        public virtual void ClearTargets()
        {
            Targets.Clear();
            ActiveTargets.Clear();
        }

        // Workaround for when all tanks are removed and there is no target left
        private void UsePlaceholderTarget()
        {
            setPlaceholder = true;
        }

        private void Update()
        {
            if (setPlaceholder)
            {
                setPlaceholder = false;
                if (placeholderTarget == null)
                {
                    placeholderTarget = new GameObject("Camera Placeholder Target");
                }

                usingPlaceholderTarget = true;
                placeholderTarget.transform.position = transform.position;
                Targets.Add(placeholderTarget.gameObject);
            }
        }

        public void RemoveAll()
        {
            Targets.Clear();
        }
    }
}