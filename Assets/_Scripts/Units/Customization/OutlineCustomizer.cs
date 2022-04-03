using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Units.Customization
{
    [RequireComponent(typeof(Outline))]
    public class OutlineCustomizer : MonoBehaviour
    {
        [SerializeField, Required] private Outline outline;

        private void Awake()
        {
            if (!outline) outline = GetComponent<Outline>();
        }

        public void EnableOutline()
        {
            outline.enabled = true;
        }
        
        public void DisableOutline()
        {
            outline.enabled = false;
        }

        public void ChangeColor(Color color)
        {
            outline.OutlineColor = color;
        }

        private void OnValidate()
        {
            if (!outline) outline = GetComponent<Outline>();
        }
    }
}