using Systems.Sound;
using UnityEngine;

namespace Systems.Network
{
    // This class is used so we can load all systems at the start instead of loading them during the game
    public class CreatSystemsIfDoNotExist : MonoBehaviour
    {
        private void Start()
        {
            NetworkSystem.CreateIfDoesNotExist();
            LevelSystem.CreateIfDoesNotExist();
            PlayerSystem.CreateIfDoesNotExist();
            RumbleSystem.CreateIfDoesNotExist();
            SettingsSystem.CreateIfDoesNotExist();
            SoundSystem.CreateIfDoesNotExist();
        }
    }
}