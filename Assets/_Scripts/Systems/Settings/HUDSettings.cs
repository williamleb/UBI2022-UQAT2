using Sirenix.OdinInspector;
using UnityEngine;

namespace Systems.Settings
{
    [CreateAssetMenu(menuName = "Settings/HUD Settings")]
    public class HUDSettings : ScriptableObject
    {
        [Tooltip("Opacity for deactivated actions")]
        [SerializeField] [MinValue(0.1f)] private float deactivatedActionOpacity = 0.3f;

        public float DeactivatedActionOpacity => deactivatedActionOpacity;
    }
}