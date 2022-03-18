using UnityEngine;

namespace Units.Player.Customisation
{
    public class CustomizationPoint : MonoBehaviour
    {
        private GameObject currentElement;

        public void LoadElement(GameObject head)
        {
            UnloadCurrentElement();

            if (head == null)
                return;

            currentElement = Instantiate(head, transform);
        }

        private void UnloadCurrentElement()
        {
            if (currentElement == null)
                return;

            Destroy(currentElement);
        }

        public void LoadMaterialOnElement(Material material, int materialIndex)
        {
            if (material == null)
                return;

            if (currentElement == null)
                return;

            var meshRenderer = currentElement.GetComponentInChildren<Renderer>();
            if (meshRenderer == null)
                return;

            if (materialIndex < 0 || materialIndex >= meshRenderer.materials.Length)
                return;

            var materials = meshRenderer.materials;
            materials[materialIndex] = material;
            meshRenderer.materials = materials;
        }
    }
}