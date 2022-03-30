using Fusion;
using Units.Player;
using UnityEngine;

namespace Units.Customization
{
    [RequireComponent(typeof(PlayerEntity))]
    [RequireComponent(typeof(PlayerCustomization))]
    public class LinkArchetypeToCustomization : NetworkBehaviour
    {
        private PlayerEntity entity;
        private PlayerCustomization customization;

        private void Awake()
        {
            entity = GetComponent<PlayerEntity>();
            customization = GetComponent<PlayerCustomization>();
        }

        public override void Spawned()
        {
            if (Object.HasInputAuthority)
            {
                UpdateCustomization();
                entity.OnArchetypeChanged += UpdateCustomization;
            }
        }

        public override void Despawned(NetworkRunner runner, bool hasState)
        {
            if (Object.HasInputAuthority)
            {
                entity.OnArchetypeChanged -= UpdateCustomization;
            }
        }

        private void UpdateCustomization()
        {
            customization.SetClothes(entity.Archetype);
        }
    }
}