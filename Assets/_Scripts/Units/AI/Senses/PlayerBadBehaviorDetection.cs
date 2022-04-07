using Systems.Settings;
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
        private AISettings settings;

        private bool deactivateUntilNextPoll;

        private PlayerEntity playerThatHadBadBehavior;

        public bool HasSeenPlayerWithBadBehavior => PlayerThatHadBadBehavior != null;
        public PlayerEntity PlayerThatHadBadBehavior
        {
            get
            {
                deactivateUntilNextPoll = false;
                return playerThatHadBadBehavior;
            }
            private set => playerThatHadBadBehavior = value;
        }

        public void DeactivateUntilNextPoll()
        {
            deactivateUntilNextPoll = true;
        }
        
        private void Awake()
        {
            vision = GetComponent<Vision>();
            aiEntity = GetComponent<AIEntity>();
            settings = SettingsSystem.AISettings;
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
            if (deactivateUntilNextPoll)
                return;
            
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
            if (deactivateUntilNextPoll)
                return;
            
            if (HasSeenPlayerWithBadBehavior)
                return;
            
            PlayerThatHadBadBehavior = null;
            foreach (var player in vision.PlayersInSight)
            {
                if (player.HasHitSomeoneThisFrame)
                {
                    PlayerThatHadBadBehavior = player;
                    return;
                }

                var speedToConsiderBadBehavior = settings.PercentOfDashConsideredBadBehavior * (player.SprintMaxSpeed - player.WalkMaxSpeed) + player.WalkMaxSpeed;
                if (player && player.CurrentSpeed > speedToConsiderBadBehavior)
                {
                    PlayerThatHadBadBehavior = player;
                    return;
                }
            }
        }
    }
}