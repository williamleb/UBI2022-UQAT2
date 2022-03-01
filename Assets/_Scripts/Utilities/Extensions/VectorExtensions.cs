using UnityEngine;

namespace Utilities.Extensions
{
    public static class VectorExtensions
    {
        /// <summary>
        /// Vector 2 from vector 3's x and y components
        /// </summary>
        public static Vector2 ToV2(this Vector3 vector) => new Vector2(vector.x, vector.y);
        
        /// <summary>
        /// Vector 2 from vector 3's x and z components
        /// </summary>
        public static Vector2 FlatV3ToV2(this Vector3 vector) => new Vector2(vector.x, vector.y);

        /// <summary>
        /// Vector 3 removing the y component
        /// </summary>
        public static Vector3 Flat(this Vector3 vector) => new Vector3(vector.x, 0, vector.z);
        
        /// <summary>
        /// Vector 3 from vector 2's x becoming x and y becoming z
        /// </summary>
        public static Vector3 V2ToFlatV3(this Vector2 vector) => new Vector3(vector.x, 0, vector.y);

        public static Vector3Int ToVector3Int(this Vector3 input) =>
            new Vector3Int((int) input.x, (int) input.y, (int) input.z);
        
        public static float SqrDistance(Vector3 a, Vector3 b)
        {
            float num1 = a.x - b.x;
            float num2 = a.y - b.y;
            float num3 = a.z - b.z;
            return (float) (num1 * (double) num1 + num2 * (double) num2 + num3 * (double) num3);
        }
        
        // Inspired of reefwirrax's answer on https://answers.unity.com/questions/532297/rotate-a-vector-around-a-certain-point.html
        public static Vector3 RotateAround(this Vector3 point, Vector3 pivot, Vector3 angles) {
            var dir = point - pivot;
            dir = Quaternion.Euler(angles) * dir;
            point = dir + pivot;
            return point;
        }
    }
}