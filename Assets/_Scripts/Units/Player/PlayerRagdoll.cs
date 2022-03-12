﻿using Fusion;
using System.Collections.Generic;
using UnityEngine;

namespace Units.Player
{
    public partial class PlayerEntity
    {
        //Vector3 = transform initial local position of bones
        //Quaternion = transform's initial local rotation of bones
        private readonly List<(Collider, Vector3, Quaternion)> ragdollColliders = new List<(Collider, Vector3, Quaternion)>();
        private readonly List<Rigidbody> ragdollRigidbody = new List<Rigidbody>();
        private Collider playerCollider;
        [SerializeField] private Transform ragdollTransform; //Used to set playerEntity transform after ragdoll.
        [SerializeField] private Transform ragdollPelvis;

        private void RagdollAwake()
        {
            InitializeRagdoll();
        }

        private void InitializeRagdoll()
        {
            var collidersInPlayer = GetComponentsInChildren<Collider>();
            foreach (Collider col in collidersInPlayer)
            {
                if (col.transform.gameObject == transform.gameObject)
                {
                    playerCollider = col;
                }
                else
                {
                    Transform colTransform = col.transform;
                    ragdollColliders.Add((col, colTransform.localPosition, colTransform.localRotation));
                    col.isTrigger = true;

                    var rigidBody = col.gameObject.GetComponent<Rigidbody>();
                    rigidBody.isKinematic = true;
                    ragdollRigidbody.Add(rigidBody);
                }
            }
        }

        [Rpc(RpcSources.StateAuthority,RpcTargets.All)]
        private void RPC_ToggleRagdoll(NetworkBool isActivate, Vector3 forceDirection = default, float forceMagnitude = default)
        {
            networkAnimator.Animator.enabled = !isActivate;
            playerCollider.enabled = !isActivate;
            
            foreach ((Collider col, Vector3 localPos, Quaternion localRot) in ragdollColliders)
            {
                col.isTrigger = !isActivate;

                //Reset bones to local position
                if (!isActivate)
                {
                    Transform elementTransform = col.transform;
                    elementTransform.localPosition = localPos;
                    elementTransform.localRotation = localRot;
                }
            }

            foreach (Rigidbody rb in ragdollRigidbody)
            {
                rb.isKinematic = !isActivate;

                if (isActivate && forceDirection != default)
                    rb.AddForce(forceDirection * (forceMagnitude != default ? forceMagnitude : Velocity.magnitude), ForceMode.Impulse);
            }
        }
    }
}