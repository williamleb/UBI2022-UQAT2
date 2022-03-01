using Units.Player;
using UnityEngine;

namespace Units.AI.Senses
{
    [RequireComponent(typeof(Vision))]
    public class PlayerHitterDetection : MonoBehaviour
    {
        private Vision vision;

        public PlayerEntity PlayerThatHitSomeoneThisFrame { get; private set; } = null;
        public bool HasAPlayerHitSomeoneThisFrame => PlayerThatHitSomeoneThisFrame != null;
        
        private void Awake()
        {
            vision = GetComponent<Vision>();
        }

        private void Update()
        {
            PlayerThatHitSomeoneThisFrame = null;
            foreach (var player in vision.PlayersInSight)
            {
                if (player.HasHitSomeoneThisFrame)
                {
                    PlayerThatHitSomeoneThisFrame = player;
                }
            }
        }
    }
}