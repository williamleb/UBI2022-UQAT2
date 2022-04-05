using System.Collections.Generic;
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
        private bool isRagdoll;

        [Header("Ragdoll")] [SerializeField]
        private Transform ragdollTransform; //Used to set playerEntity transform after ragdoll.

        [Networked(OnChanged = nameof(OnGetUpFChangedCallback))]
        private bool GetUpF { get; set; }

        [Networked(OnChanged = nameof(OnGetUpBChangedCallback))]
        private bool GetUpB { get; set; }
        private bool IsFacingUp => Vector3.Dot(ragdollTransform.forward, Vector3.up) > 0;
        
        private void RagdollAwake()
        {
            InitializeRagdoll();
        }

        private void RagdollUpdate()
        {
            if (isRagdoll)
            {
                // ReSharper disable Unity.InefficientPropertyAccess
                transform.position = Vector3.MoveTowards(transform.position, ragdollTransform.position.Flat(), 0.1f);
                ragdollTransform.position = Vector3.MoveTowards(ragdollTransform.position, transform.position, 0.1f);
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
                col.attachedRigidbody.isKinematic = true;
            }
        }

        [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
        private void RPC_ToggleRagdoll(NetworkBool isActivate, Vector3 forceDirection = default, float forceMagnitude = 0)
        {
            isRagdoll = isActivate;

            networkAnimator.Animator.enabled = false;

            if (!isActivate)
            {
                GetUpB = IsFacingUp;
                GetUpF = !GetUpB;
                
                Vector3 euler = transform.eulerAngles;
                euler.y = ragdollTransform.eulerAngles.y;
                transform.eulerAngles = euler;
                lastMoveDirection = transform.forward;
            }
            
            foreach (BodyPart bp in ragdollColliders)
            {
                bp.Collider.isTrigger = !isActivate;
                bp.Rb.isKinematic = !isActivate;

                if (isActivate && forceDirection != default)
                    bp.Rb.AddForce(forceDirection.normalized * (forceMagnitude == 0 ? Velocity.magnitude : forceMagnitude), ForceMode.Impulse);
            }

            if (isActivate)
            {
                PlayFumbleSoundLocally();
                PlayHitFXLocally();
            }
        }
        
        
        private void ResetGetUp()
        {
            GetUpF = false;
            GetUpB = false;

            foreach (BodyPart bp in ragdollColliders)
            {
                Transform elementTransform = bp.Collider.transform;
                elementTransform.localPosition = bp.Position;
                elementTransform.localRotation = bp.Rotation;
            }
        }
        
        private static void OnGetUpFChangedCallback(Changed<PlayerEntity> changed)
        {
            changed.Behaviour.networkAnimator.Animator.enabled = true;
            changed.Behaviour.AnimationSetBool(GetUpFAnim, changed.Behaviour.GetUpF);
            if (changed.Behaviour.GetUpF)
                changed.Behaviour.StartCoroutine(changed.Behaviour.AfterGetUp(changed.Behaviour.GetUpF));
        }
        private static void OnGetUpBChangedCallback(Changed<PlayerEntity> changed)
        {
            changed.Behaviour.networkAnimator.Animator.enabled = true;
            changed.Behaviour.AnimationSetBool(GetUpBAnim, changed.Behaviour.GetUpB);
            if (changed.Behaviour.GetUpB)
                changed.Behaviour.StartCoroutine(changed.Behaviour.AfterGetUp(changed.Behaviour.GetUpF));
        }
    }
}