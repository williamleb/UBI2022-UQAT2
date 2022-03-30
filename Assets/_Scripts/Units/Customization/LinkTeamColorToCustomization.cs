using Fusion;
using Systems.Teams;
using Units.Player;
using UnityEngine;
using Utilities.Extensions;

namespace Units.Customization
{
    [RequireComponent(typeof(PlayerEntity))]
    [RequireComponent(typeof(PlayerCustomization))]
    public class LinkTeamColorToCustomization : NetworkBehaviour
    {
        private PlayerEntity entity;
        private PlayerCustomization customization;
        private Team currentTeam;

        private void Awake()
        {
            entity = GetComponent<PlayerEntity>();
            customization = GetComponent<PlayerCustomization>();
        }

        public override void Spawned()
        {
            if (Object.HasInputAuthority)
            {
                UpdateTeam();
                entity.OnTeamChanged += UpdateTeam;
            }
        }

        public override void Despawned(NetworkRunner runner, bool hasState)
        {
            if (Object.HasInputAuthority)
            {
                if (currentTeam)
                {
                    currentTeam.OnColorChanged -= UpdateCustomization;
                }

                entity.OnTeamChanged -= UpdateTeam;
            }
        }

        private void UpdateTeam()
        {
            if (currentTeam)
            {
                currentTeam.OnColorChanged -= UpdateCustomization;
            }

            if (!entity.TeamId.IsNullOrEmpty())
                currentTeam = TeamSystem.Instance.GetTeam(entity.TeamId);

            if (currentTeam)
            {
                UpdateCustomization(currentTeam.Color);
                currentTeam.OnColorChanged += UpdateCustomization;
            }
        }

        private void UpdateCustomization(int color)
        {
            customization.SetClothesColor(color);
        }
    }
}