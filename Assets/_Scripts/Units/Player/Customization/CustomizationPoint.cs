using UnityEngine;

namespace Units.Player.Customisation
{
    public class CustomizationPoint : MonoBehaviour
    {
        private GameObject currentHead;

        public void LoadHead(GameObject head)
        {
            UnloadCurrentHead();

            if (head == null)
                return;

            currentHead = Instantiate(head, transform);
        }

        private void UnloadCurrentHead()
        {
            if (currentHead == null)
                return;

            Destroy(currentHead);
        }

        public void LoadMaterialOnHead(Material material, int materialIndex)
        {
            if (material == null)
                return;

            if (currentHead == null)
                return;

            var renderer = currentHead.GetComponentInChildren<Renderer>();
            if (renderer == null)
                return;

            if (materialIndex < 0 || materialIndex >= renderer.materials.Length)
                return;

            var materials = renderer.materials;
            materials[materialIndex] = material;
            renderer.materials = materials;
        }
    }
}