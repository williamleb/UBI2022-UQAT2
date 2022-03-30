using Systems.Settings;
using Units.Player;
using UnityEngine;

namespace Units.Customization
{
    public class ClothesColorCustomizer : MaterialCustomizer
    {
        [SerializeField] private Archetype targetArchetype;
        [SerializeField] private ClothesType clothesType;

        public Archetype TargetArchetype => targetArchetype;
        public ClothesType ClothesType => clothesType;
    }
}