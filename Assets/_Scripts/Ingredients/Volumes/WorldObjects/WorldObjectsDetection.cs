using System;
using System.Collections.Generic;
using System.Linq;
using Fusion;
using Ingredients.Homework;
using Systems;
using Units.AI;
using Units.Player;
using UnityEngine;
using Utilities.Extensions;

namespace Ingredients.Volumes.WorldObjects
{
    [RequireComponent(typeof(Collider))]
    public class WorldObjectsDetection : MonoBehaviour
    {
        private const float SECONDS_BETWEEN_UPDATES = 1f;


        public event Action<IWorldObject> OnWorldObjectEntered;
        public event Action<IWorldObject> OnWorldObjectLeft;
        
        private Collider col;

        private Dictionary<IWorldObject, bool> worldObjects = new Dictionary<IWorldObject, bool>();

        private void Awake()
        {
            col = GetComponent<Collider>();
        }

        private void Start()
        {
            InitWithExistingWorldObjects();
            
            PlayerEntity.OnPlayerSpawned += AddWorldObjectFromPlayer;
            PlayerEntity.OnPlayerDespawned += RemoveWorldObjectFromPlayer;
            AIEntity.OnAISpawned += AddWorldObject;
            AIEntity.OnAIDespawned += RemoveWorldObject;
            Homework.Homework.OnHomeworkSpawned += AddWorldObject;
            Homework.Homework.OnHomeworkDespawned += RemoveWorldObject;
            
            InvokeRepeating(nameof(UpdateWorldObjectState),SECONDS_BETWEEN_UPDATES, SECONDS_BETWEEN_UPDATES);
        }

        private void OnDestroy()
        {
            PlayerEntity.OnPlayerSpawned -= AddWorldObjectFromPlayer;
            PlayerEntity.OnPlayerDespawned -= RemoveWorldObjectFromPlayer;
            AIEntity.OnAISpawned -= AddWorldObject;
            AIEntity.OnAIDespawned -= RemoveWorldObject;
            Homework.Homework.OnHomeworkSpawned -= AddWorldObject;
            Homework.Homework.OnHomeworkDespawned -= RemoveWorldObject;
            
            worldObjects.Clear();
        }

        private void InitWithExistingWorldObjects()
        {
            foreach (var player in PlayerSystem.Instance.AllPlayers)
            {
                AddWorldObject(player);
            }

            if (AIManager.HasInstance)
            {
                foreach (var ai in AIManager.Instance.AllAIs)
                {
                    AddWorldObject(ai);
                }
            }

            if (HomeworkManager.HasInstance)
            {
                foreach (var homework in HomeworkManager.Instance.Homeworks)
                {
                    AddWorldObject(homework);
                }
            }

        }

        private void AddWorldObjectFromPlayer(NetworkObject networkObject) => AddWorldObject(networkObject.GetComponentInEntity<PlayerEntity>());
        private void RemoveWorldObjectFromPlayer(NetworkObject networkObject) => RemoveWorldObject(networkObject.GetComponentInEntity<PlayerEntity>());

        private void AddWorldObject(IWorldObject worldObject)
        {
            if (worldObjects.ContainsKey(worldObject))
                return;
            
            worldObjects.Add(worldObject, false);
        }

        private void RemoveWorldObject(IWorldObject worldObject)
        {
            worldObjects.Remove(worldObject);
        }

        private void UpdateWorldObjectState()
        {
            foreach (var worldObject in worldObjects.ToList())
            {
                SetIsIn(worldObject.Key, col.bounds.Contains(worldObject.Key.Position));
            }
        }

        private void SetIsIn(IWorldObject worldObject, bool value)
        {
            if (!worldObjects.ContainsKey(worldObject)) 
                return;
            
            if (value == worldObjects[worldObject])
                return;
                
            worldObjects[worldObject] = value;
            if (value) OnWorldObjectEntered?.Invoke(worldObject);
            else OnWorldObjectLeft?.Invoke(worldObject);
        }
    }
}