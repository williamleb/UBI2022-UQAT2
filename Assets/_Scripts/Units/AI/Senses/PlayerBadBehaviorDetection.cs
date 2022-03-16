using System;
using Units.Player;
using UnityEngine;
using Utilities.Extensions;

namespace Units.AI.Senses
{
    [RequireComponent(typeof(Vision))]
    [RequireComponent(typeof(AIEntity))]
    public class PlayerBadBehaviorDetection : MonoBehaviour
    {
        private Vision vision;
        private AIEntity aiEntity;

        public PlayerEntity PlayerThatHadBadBehavior { get; private set; } = null;
        public bool HasSeenPlayerWithBadBehavior => PlayerThatHadBadBehavior != null;
        
        private void Awake()
        {
            vision = GetComponent<Vision>();
            aiEntity = GetComponent<AIEntity>();
        }

        private void Start()
        {
            aiEntity.OnHit += OnHit;
        }

        private void OnDestroy()
        {
            aiEntity.OnHit -= OnHit;
        }

        private void OnHit(GameObject hitter)
        {
            if (!hitter.IsAPlayer()) 
                return;
            
            var player = hitter.GetComponentInEntity<PlayerEntity>();
            PlayerThatHadBadBehavior = player;
        }

        public void MarkBadBehaviorAsSeen()
        {
            PlayerThatHadBadBehavior = null;
        }

        private void Update()
        {
            if (HasSeenPlayerWithBadBehavior)
                return;
            
            PlayerThatHadBadBehavior = null;
            foreach (var player in vision.PlayersInSight)
            {
                if (player.HasHitSomeoneThisFrame)
                {
                    PlayerThatHadBadBehavior = player;
                }
                
                // TODO Detect sprint
                // TODO Detect dash in him
                
                Debug.Log($"Player velocity: {player.Velocity.magnitude}");
            }
        }
    }
}