using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Environment.Randomizer
{
    [RequireComponent(typeof(MeshRenderer))]
    public class MaterialRandomizer : Randomizer
    {
        [SerializeField, PreviewField] private List<Material> possibleMaterials = new List<Material>();

        private MeshRenderer meshRenderer;
        
        protected override int NumberOfElements => possibleMaterials.Count;

        private void Awake()
        {
            meshRenderer = GetComponent<MeshRenderer>();
        }

        protected override void UpdateElement(int elementNumber)
        {
            meshRenderer.material = possibleMaterials[elementNumber];
        }
        
#if UNITY_EDITOR
        protected override void EditorUpdateElement(int elementNumber)
        {
            var editorMeshRenderer = GetComponent<MeshRenderer>();
            editorMeshRenderer.material = possibleMaterials[elementNumber]; 
        }
#endif
    }
}