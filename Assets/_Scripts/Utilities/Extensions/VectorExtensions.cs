using UnityEngine;

namespace Utilities.Extensions
{
    public static class VectorExtensions
    {
        public static Vector2 ToV2(this Vector3 input) => new Vector2(input.x, input.y);

        public static Vector3 Flat(this Vector3 input) => new Vector3(input.x, 0, input.z);
        public static Vector3 V2ToFlatV3(this Vector2 input) => new Vector3(input.x, 0, input.y);

        public static Vector3Int ToVector3Int(this Vector3 input) =>
            new Vector3Int((int) input.x, (int) input.y, (int) input.z);
        
        public static float SqrDistance(Vector3 a, Vector3 b)
        {
            float num1 = a.x - b.x;
            float num2 = a.y - b.y;
            float num3 = a.z - b.z;
            return (float) (num1 * (double) num1 + num2 * (double) num2 + num3 * (double) num3);
        }
    }
}