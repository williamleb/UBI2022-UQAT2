﻿using System.Collections.Generic;
using Fusion;
using UnityEngine;
using Utilities.Extensions;

namespace Units.Player
{
    public partial class PlayerEntity
    {
        //Vector3 = transform initial local position of bones
        //Quaternion = transform's initial local rotation of bones
        private readonly List<BodyPart> ragdollColliders = new List<BodyPart>();
        private readonly List<Rigidbody> ragdollRigidbody = new List<Rigidbody>();
        private bool isRagdoll;

        [Header("Ragdoll")] [SerializeField]
        private Transform ragdollTransform; //Used to set playerEntity transform after ragdoll.

        [SerializeField] private Transform ragdollPelvis;

        private void RagdollAwake()
        {
            InitializeRagdoll();
        }

        private void RagdollUpdate()
        {
            if (isRagdoll)
            {
                Vector3 ragdollPos = ragdollTransform.position;
                Vector3 pos = transform.position;
                transform.position = Vector3.MoveTowards(pos, ragdollPos.Flat(), 0.1f);
                ragdollTransform.position = Vector3.MoveTowards(ragdollPos, pos, 0.1f);
            }
        }

        private void InitializeRagdoll()
        {
            Collider[] collidersInPlayer = GetComponentsInChildren<Collider>();
            foreach (Collider col in collidersInPlayer)
            {
                if (col.transform.gameObject == transform.gameObject) continue;

                Transform colTransform = col.transform;
                ragdollColliders.Add(new BodyPart
                    {Collider = col, Position = colTransform.localPosition, Rotation = colTransform.localRotation});
                col.isTrigger = true;

                Rigidbody rb = col.attachedRigidbody;
                rb.isKinematic = true;
                ragdollRigidbody.Add(rb);
            }
        }

        [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
        private void RPC_ToggleRagdoll(NetworkBool isActivate, Vector3 forceDirection = default,
            float forceMagnitude = default)
        {
            isRagdoll = isActivate;

            networkAnimator.Animator.enabled = !isActivate;
            AnimationUpdate();

            foreach (BodyPart bp in ragdollColliders)
            {
                bp.Collider.isTrigger = !isActivate;
                //Reset bones to local position
                if (!isActivate)
                {
                    Transform elementTransform = bp.Collider.transform;
                    elementTransform.localPosition = bp.Position;
                    elementTransform.localRotation = bp.Rotation;
                }
            }
            
            foreach (Rigidbody rb in ragdollRigidbody)
            {
                rb.isKinematic = !isActivate;

                if (isActivate && forceDirection != default)
                    rb.AddForce(forceDirection.normalized * forceMagnitude, ForceMode.Impulse);
            }

            if (isActivate)
                PlayFumbleSoundLocally();
        }
    }
}