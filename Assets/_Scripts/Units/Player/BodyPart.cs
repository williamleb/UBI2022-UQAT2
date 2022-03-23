using UnityEngine;

namespace Units.Player
{
    public struct BodyPart
    {
        public Collider Collider;
        public Vector3 Position;
        public Quaternion Rotation;
        public Rigidbody Rb => Collider.attachedRigidbody;
    }
}