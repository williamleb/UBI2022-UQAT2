using Fusion;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Environment.Randomizer
{
    public abstract class Randomizer : NetworkBehaviour
    {
        [SerializeField] private bool randomizeAtStart = true;
        
        [Networked(OnChanged = nameof(OnCurrentElementIndexChanged)), HideInInspector] 
        private int CurrentElementIndex { get; set; }
        
        protected abstract int NumberOfElements { get; }
        protected abstract void UpdateElement(int elementNumber);

        public override void Spawned()
        {
            base.Spawned();

            if (Object.HasStateAuthority)
            {
                if (randomizeAtStart)
                    Randomize();
                
                RPC_ForceUpdateAllClients();
            }
            
            UpdateElement();
        }

        public void Randomize()
        {
            if (!Object.HasStateAuthority)
                return;

            if (NumberOfElements <= 0)
                return;
            
            CurrentElementIndex = Random.Range(0, NumberOfElements);
        }

        [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
        private void RPC_ForceUpdateAllClients()
        {
            UpdateElement();
        }

        private void UpdateElement()
        {
            if (CurrentElementIndex >= 0 && CurrentElementIndex < NumberOfElements)
                UpdateElement(CurrentElementIndex);
        }

        private static void OnCurrentElementIndexChanged(Changed<Randomizer> changed)
        {
            changed.Behaviour.UpdateElement();
        }
        
#if UNITY_EDITOR
        [Button("Randomize")]
        private void EditorRandomize()
        {
            if (NumberOfElements <= 0)
                return;
            
            EditorUpdateElement(Random.Range(0, NumberOfElements));
        }

        protected virtual void EditorUpdateElement(int elementNumber) { }
#endif
    }
}