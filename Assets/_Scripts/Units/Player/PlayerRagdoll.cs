using Fusion;
using System.Collections.Generic;
using UnityEngine;

namespace Units.Player
{
    public partial class PlayerEntity
    {
        //Vector3 = transform initial local position of bones
        //Quaternion = transform's initial local rotation of bones
        private List<(Collider, Vector3, Quaternion)> ragdollColliders = new List<(Collider, Vector3, Quaternion)>();
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
                    Transform colTransform = collider.transform;
                    ragdollColliders.Add((collider, colTransform.localPosition, colTransform.localRotation));
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

            //Item1 = collider, Item2 = local position, Item3 = local rotation
            foreach ((Collider, Vector3, Quaternion) element in ragdollColliders)
            {
                element.Item1.isTrigger = !IsActivate;

                //Reset bones to local position
                if (!IsActivate)
                {
                    Transform elementTransform = element.Item1.transform;
                    elementTransform.localPosition = element.Item2;
                    elementTransform.localRotation = element.Item3;
                }
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