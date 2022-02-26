using System.Collections;
using System.Collections.Generic;
using Systems.Network;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Matchmaking
{
    public class GameLauncher : MonoBehaviour
    {
        [SerializeField] private GameObject HostUI;
        [SerializeField] private GameObject ClientUI;
        [SerializeField] private GameObject UICurtain;

        [SerializeField] private TextMeshProUGUI createGameTextField;
        [SerializeField] private TextMeshProUGUI joinGameTextField;

        // Start is called before the first frame update
        void Start()
        {
            NetworkSystem.Instance.DebugMode = false;
        }

        // Update is called once per frame
        void Update()
        {

        }

        public void CreateGameClick()
        {
            Debug.Log($"Create game click");
            _ = NetworkSystem.Instance.CreateGame(createGameTextField.text);
        }

        public async void JoinGameClick()
        {
            Debug.Log($"Join game click");

            if (!await NetworkSystem.Instance.TryJoinGame(joinGameTextField.text))
            {

            }
        }
    }
}
