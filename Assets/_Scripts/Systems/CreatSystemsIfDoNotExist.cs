using Canvases.TransitionScreen;
using Systems.Level;
using Systems.Network;
using Systems.Sound;
using UnityEngine;

namespace Systems.Settings
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
            TransitionScreenSystem.CreateIfDoesNotExist();
        }
    }
}