using System;
using AK.Wwise;
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
        [SerializeField] private Event dash;
        [SerializeField] private Event dashCollision;
        
        [SerializeField] private Event handInHomework;
        [SerializeField] private Event pickupHomework;
        [SerializeField] private Event interactWorldElement;
        [SerializeField] private Event characterSelectBase;
        [SerializeField] private Event characterSelectRunner;
        [SerializeField] private Event characterSelectThrower;
        [SerializeField] private Event characterSelectDasher;
        
        [SerializeField] private Event aimHold;
        [SerializeField] private Event aimRelease;
        [SerializeField] private Event homeworkFlying;

        [SerializeField] private Event victoryJingle;
        [SerializeField] private Event defeatJingle;

        [SerializeField] private Event three;
        [SerializeField] private Event two;
        [SerializeField] private Event one;
        [SerializeField] private Event go;
        
        [SerializeField] private Event janitorCaughtAlert;
        
        [SerializeField] private Event menuElementSelect;
        [SerializeField] private Event menuElementForward;
        [SerializeField] private Event menuElementBackward;

        
        [Header("RTCP")] 
        [SerializeField] private RTPC masterVolume;
        [SerializeField] private RTPC musicVolume;
        [SerializeField] private RTPC sfxVolume;
        [SerializeField] private RTPC aimCharge;
        
        [Header("Music")]
        [SerializeField] private Event music;

        [Header("States")] 
        [SerializeField] private State inMenu;
        [SerializeField] private State inGame;
        
        public GameObject SoundBankPrefab => soundBankPrefab;
        
        public Event FootstepEvent => footstep;
        public Event FumbleEvent => fumble;
        public Event DashEvent => dash;
        public Event DashCollisionEvent => dashCollision;
        public Event HandInHomeworkEvent => handInHomework;
        public Event PickUpHomeworkEvent => pickupHomework;
        public Event InteractWorldElementEvent => interactWorldElement;
        public Event CharacterSelectBaseEvent => characterSelectBase;
        public Event CharacterSelectRunnerEvent => characterSelectRunner;
        public Event CharacterSelectThrowerEvent => characterSelectThrower;
        public Event CharacterSelectDasherEvent => characterSelectDasher;
        public Event AimHoldEvent => aimHold;
        public Event AimReleaseEvent => aimRelease;
        public Event HomeworkFlyingEvent => homeworkFlying;
        public Event VictoryJingleEvent => victoryJingle;
        public Event DefeatJingleEvent => defeatJingle;
        public Event OneEvent => one;
        public Event TwoEvent => two;
        public Event ThreeEvent => three;
        public Event GoEvent => go;
        public Event JanitorCaughtAlertEvent => janitorCaughtAlert;
        public Event MenuElementSelectEvent => menuElementSelect;
        public Event MenuElementForwardEvent => menuElementForward;
        public Event MenuElementBackwardEvent => menuElementBackward;

        public RTPC MasterVolumeParameter => masterVolume;
        public RTPC MusicVolumeParameter => musicVolume;
        public RTPC SoundEffectsVolumeParameter => sfxVolume;
        public RTPC AimChargeParameter => aimCharge;
        
        public Event MusicEvent => music;
        
        public State InMenuState => inMenu;
        public State InGameState => inGame;
    }
}
