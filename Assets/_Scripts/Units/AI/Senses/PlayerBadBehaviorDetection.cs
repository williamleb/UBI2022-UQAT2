using Units.Player;
using UnityEngine;

namespace Units.AI.Senses
{
    [RequireComponent(typeof(Vision))]
    public class PlayerBadBehaviorDetection : MonoBehaviour
    {
        private Vision vision;

        public PlayerEntity PlayerHadBadBehaviorThisFrame { get; private set; } = null;
        public bool HasAPlayerHaveBadBehaviorThisFrame => PlayerHadBadBehaviorThisFrame != null;
        
        private void Awake()
        {
            vision = GetComponent<Vision>();
        }

        private void Update()
        {
            PlayerHadBadBehaviorThisFrame = null;
            foreach (var player in vision.PlayersInSight)
            {
                if (player.HasHitSomeoneThisFrame)
                {
                    PlayerHadBadBehaviorThisFrame = player;
                }
                
                // TODO Detect sprint
                // TODO Detect dash in him
                
                Debug.Log($"Player velocity: {player.Velocity.magnitude}");
            }
        }
    }
}