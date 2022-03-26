using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Environment.Randomizer
{
    [RequireComponent(typeof(MeshFilter))]
    public class MeshRandomizer : Randomizer
    {
        [SerializeField, PreviewField] private List<Mesh> possibleMeshes = new List<Mesh>();

        private MeshFilter meshFilter;
        
        protected override int NumberOfElements => possibleMeshes.Count;

        private void Awake()
        {
            meshFilter = GetComponent<MeshFilter>();
        }

        protected override void UpdateElement(int elementNumber)
        {
            meshFilter.mesh = possibleMeshes[elementNumber];
        }
        
#if UNITY_EDITOR
        protected override void EditorUpdateElement(int elementNumber)
        {
            var editorMeshFilter = GetComponent<MeshFilter>();
            editorMeshFilter.mesh = possibleMeshes[elementNumber];
        }
#endif
    }
}