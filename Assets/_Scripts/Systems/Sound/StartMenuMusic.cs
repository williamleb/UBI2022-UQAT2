using System;
using UnityEngine;

namespace Systems.Sound
{
    public class StartMenuMusic : MonoBehaviour
    {
        private void Start()
        {
            SoundSystem.Instance.PlayVictoryJingleSound(); // TODO Change for game music when we will have it
        }
    }
}