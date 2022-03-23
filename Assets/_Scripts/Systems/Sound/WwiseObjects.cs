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
        [SerializeField] private Event footstep;
        [SerializeField] private Event fumble;
        [SerializeField] private Event handInHomework;
        [SerializeField] private Event pickupHomework;
        
        public GameObject SoundBankPrefab => soundBankPrefab;
        
        public Event FootstepEvent => footstep;
        public Event FumbleEvent => fumble;
        public Event HandInHomeworkEvent => handInHomework;
        public Event PickUpHomeworkEvent => pickupHomework;
    }
}
