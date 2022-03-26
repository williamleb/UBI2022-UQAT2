using Systems.Settings;
using UnityEngine;

namespace Units.Player.Customisation
{
    public class ClothesColorCustomizer : MaterialCustomizer
    {
        [SerializeField] private Archetype targetArchetype;
        [SerializeField] private ClothesType clothesType;

        public Archetype TargetArchetype => targetArchetype;
        public ClothesType ClothesType => clothesType;
    }
}