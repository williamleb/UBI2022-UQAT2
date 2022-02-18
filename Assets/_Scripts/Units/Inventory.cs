using Canvases.Markers;
using Fusion;
using UnityEngine;

namespace Units
{
    public class Inventory : NetworkBehaviour
    {
        [SerializeField] private SpriteMarkerReceptor marker;
        
        
        
        public void Drop()
        {
            // TODO
            Debug.Log($"Drop {gameObject.name}");
        }
    }
}