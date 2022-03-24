﻿using System;
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
        [SerializeField] private Event handInHomework;
        [SerializeField] private Event pickupHomework;
        [SerializeField] private Event aimHold;
        [SerializeField] private Event aimRelease;

        [Header("RTCP")] 
        [SerializeField] private RTPC masterVolume;
        [SerializeField] private RTPC musicVolume;
        [SerializeField] private RTPC sfxVolume;
        [SerializeField] private RTPC aimCharge;
        
        [Header("Switch")] 
        [SerializeField] private Switch aimChargeDefault;


        public GameObject SoundBankPrefab => soundBankPrefab;
        
        public Event FootstepEvent => footstep;
        public Event FumbleEvent => fumble;
        public Event DashEvent => dash;
        public Event HandInHomeworkEvent => handInHomework;
        public Event PickUpHomeworkEvent => pickupHomework;
        public Event AimHoldEvent => aimHold;
        public Event AimReleaseEvent => aimRelease;

        public RTPC MasterVolumeParameter => masterVolume;
        public RTPC MusicVolumeParameter => musicVolume;
        public RTPC SoundEffectsVolumeParameter => sfxVolume;
        public RTPC AimChargeParameter => aimCharge;
    }
}
