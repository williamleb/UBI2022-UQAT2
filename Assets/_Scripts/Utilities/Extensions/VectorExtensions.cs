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
    }
}