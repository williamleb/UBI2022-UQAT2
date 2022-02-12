using Sirenix.OdinInspector;
using UnityEngine;
using Utilities.Singleton;

namespace Canvases.Markers
{
    public class MarkerManager : Singleton<MarkerManager>
    {
        [SerializeField, Required] private MarkerManagerData data;

        private void Awake()
        {
            
        }
    }
}