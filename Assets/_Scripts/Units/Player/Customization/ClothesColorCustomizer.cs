using UnityEngine;

namespace Units.Player.Customisation
{
    public class ClothesColorCustomizer : MaterialCustomizer
    {
        [SerializeField] private Archetype targetArchetype;

        public Archetype TargetArchetype => targetArchetype;
    }
}