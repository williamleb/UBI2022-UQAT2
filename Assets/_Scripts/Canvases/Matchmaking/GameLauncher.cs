using Systems.Network;
using TMPro;
using UnityEngine;

namespace Matchmaking
{
    public class GameLauncher : MonoBehaviour
    {
        [SerializeField] private GameObject HostUI;
        [SerializeField] private GameObject ClientUI;
        [SerializeField] private GameObject UICurtain;

        [SerializeField] private TextMeshProUGUI createGameTextField;
        [SerializeField] private TextMeshProUGUI joinGameTextField;

        void Start()
        {
            NetworkSystem.Instance.DebugMode = false;
        }

        public void CreateGameClick()
        {
            Debug.Log($"Create game click");
            _ = NetworkSystem.Instance.CreateGame(createGameTextField.text);
        }

        public async void JoinGameClick()
        {
            Debug.Log($"Join game click");
            await NetworkSystem.Instance.TryJoinGame(joinGameTextField.text);
        }
    }
}
