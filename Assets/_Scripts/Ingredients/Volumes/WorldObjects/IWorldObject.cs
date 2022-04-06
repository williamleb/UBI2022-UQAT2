using UnityEngine;

namespace Ingredients.Volumes.WorldObjects
{
    public interface IWorldObject
    {
        public Vector3 Position { get; }
        public void OnEscapedWorld();
    }
}