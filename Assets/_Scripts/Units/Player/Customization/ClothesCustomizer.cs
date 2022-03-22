using UnityEngine;

namespace Units.Player.Customisation
{
    public class ClothesCustomizer : MonoBehaviour
    {
        [SerializeField] private Archetype targetArchetype;

        public void Activate(Archetype archetype)
        {
            foreach (var meshRenderer in GetComponentsInChildren<Renderer>())
            {
                meshRenderer.enabled = archetype == targetArchetype;
            }
        }
    }
}