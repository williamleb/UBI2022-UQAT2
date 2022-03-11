using Fusion;
using System.Collections.Generic;
using UnityEngine;

namespace Units.Player
{
    public partial class PlayerEntity
    {
        //Vector3 = transform initial local position of bones
        private List<(Collider, Vector3)> ragdollColliders = new List<(Collider, Vector3)>();
        private List<Rigidbody> ragdollRigidbody = new List<Rigidbody>();
        private Collider playerCollider;
        [SerializeField] private Transform ragdollTransform; //Used to set playerEntity transform after ragdoll.

        private void RagdollAwake()
        {
            InitializeRagdoll();
        }

        private void InitializeRagdoll()
        {
            var collidersInPlayer = GetComponentsInChildren<Collider>();
            foreach (Collider collider in collidersInPlayer)
            {
                if (collider.transform.gameObject == transform.gameObject)
                {
                    playerCollider = collider;
                }
                else
                {
                    ragdollColliders.Add((collider, collider.gameObject.transform.localPosition));
                    collider.isTrigger = true;

                    var rigidBody = collider.gameObject.GetComponent<Rigidbody>();
                    rigidBody.isKinematic = true;
                    ragdollRigidbody.Add(rigidBody);
                }
            }
        }

        [Rpc(RpcSources.StateAuthority,RpcTargets.All)]
        public void RPC_ToggleRagdoll(NetworkBool IsActivate, Vector3 forceDirection = default, float forceMagnitude = default)
        {
            networkAnimator.Animator.enabled = !IsActivate;
            playerCollider.enabled = !IsActivate;

            //Item1 = collider, Item2 = local position
            foreach ((Collider, Vector3) colliderAndLocalPosition in ragdollColliders)
            {
                colliderAndLocalPosition.Item1.isTrigger = !IsActivate;

                //Reset bones to local position
                if (!IsActivate)
                    colliderAndLocalPosition.Item1.gameObject.transform.localPosition = colliderAndLocalPosition.Item2;
            }

            foreach (Rigidbody rigidbody in ragdollRigidbody)
            {
                rigidbody.isKinematic = !IsActivate;

                if (IsActivate && forceDirection != default)
                    rigidbody.AddForce(forceDirection * (forceMagnitude != default ? forceMagnitude : Velocity.magnitude), ForceMode.Impulse);
            }
        }
    }
}