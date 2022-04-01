using UnityEngine;

namespace Utilities.LayerUtils
{
    // Class modified from https://answers.unity.com/questions/609385/type-for-layer-selection.html
    [System.Serializable]
    public class SingleUnityLayer
    {
        [SerializeField]
        private int layerIndex = 0;
        public int LayerIndex => layerIndex;

        public void Set(int newLayerIndex)
        {
            if (newLayerIndex > 0 && newLayerIndex < 32)
            {
                layerIndex = newLayerIndex;
            }
        }
 
        public int Mask => 1 << layerIndex;
    }
}