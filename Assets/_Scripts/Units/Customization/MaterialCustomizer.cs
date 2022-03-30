using Sirenix.OdinInspector;
using UnityEngine;

namespace Units.Customization
{
    [RequireComponent(typeof(Renderer))]
    public class MaterialCustomizer : MonoBehaviour
    {
        [SerializeField, MinValue(0)] private int materialIndex;

        private Renderer meshRenderer;

        private void Awake()
        {
            meshRenderer = GetComponent<Renderer>();
        }

        public void LoadMaterial(Material material)
        {
            if (material == null)
                return;

            if (materialIndex < 0 || materialIndex >= meshRenderer.materials.Length)
                return;

            var materials = meshRenderer.materials;
            materials[materialIndex] = material;
            meshRenderer.materials = materials;
        }
    }
}