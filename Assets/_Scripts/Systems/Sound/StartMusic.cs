using System;
using UnityEngine;

namespace Systems.Sound
{
    public class StartMusic : MonoBehaviour
    {
        private enum State {Menu, Game}

        [SerializeField] private State state;

        private void Start()
        {
            ActivateGameState();
            SoundSystem.Instance.PlayMusic();
        }

        private void ActivateGameState()
        {
            switch (state)
            {
                case State.Menu:
                    SoundSystem.Instance.SetInMenu();
                    break;
                case State.Game:
                    SoundSystem.Instance.SetInGame();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}