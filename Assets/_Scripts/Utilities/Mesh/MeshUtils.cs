using UnityEngine;

namespace Utilities.Mesh
{
    public static class MeshUtils
    {
        public static UnityEngine.Mesh CreateQuadMesh(Vector3 upperLeftCorner, Vector3 upperRightCorner, Vector3 lowerRightCorner, Vector3 lowerLeftCorner)
        {
            return new UnityEngine.Mesh
            {
                vertices = new[] {upperLeftCorner, upperRightCorner, lowerRightCorner, lowerLeftCorner},
                normals = new[] {Vector3.up, Vector3.up, Vector3.up, Vector3.up},
                uv = new [] {new Vector2(0, 0), new Vector2(1, 0), new Vector2(0, 1), new Vector2(1, 1)},
                triangles = new[] {0, 1, 3, 2, 3, 1}
            };
        }
    }
}