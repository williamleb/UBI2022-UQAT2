using UnityEngine;
using Event = AK.Wwise.Event;

namespace Systems.Sound
{
    [CreateAssetMenu(menuName = "Wwise/Wwise Objects")]
    public class WwiseObjects : ScriptableObject
    {
        [Header("SoundBank")] 
        [SerializeField] private GameObject soundBankPrefab;
        
        [Header("SFX")]
        [SerializeField] private Event bababooey;
        
        public GameObject SoundBankPrefab => soundBankPrefab;
        
        public Event BababooeyEvent => bababooey;
    }
}
