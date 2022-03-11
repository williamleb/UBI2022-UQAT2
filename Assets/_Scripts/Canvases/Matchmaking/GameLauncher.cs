using System.Collections;
using Systems.Network;
using Systems.Settings;
using TMPro;
using UnityEngine;

namespace Matchmaking
{
    public class GameLauncher : MonoBehaviour
    {

        private MatchmakingSettings data;

        [SerializeField] private GameObject hostUI;
        [SerializeField] private GameObject clientUI;
        [SerializeField] private GameObject UICurtain;
        [SerializeField] private GameObject menuCanvas;
        
        [SerializeField] private TextMeshProUGUI createGameTextField;
        [SerializeField] private TextMeshProUGUI joinGameTextField;
        [SerializeField] private TextMeshProUGUI errorMessage;

        void Start()
        {
            data = SettingsSystem.MatchmakingSettings;
            NetworkSystem.Instance.DebugMode = false;
        }

        public async void CreateGameClick()
        {
            errorMessage.gameObject.SetActive(false);
            hostUI.SetActive(false);
            UICurtain.SetActive(true);
           
            var isGameCreated = await NetworkSystem.Instance.CreateGame(createGameTextField.text);
            
            if (isGameCreated == true)
            {
                menuCanvas.SetActive(false);
            }
            else
            {
                StartCoroutine(DisplayErrorMessage(data.ErrorMessageCreatingGame));
                UICurtain.SetActive(false);
                hostUI.SetActive(true);
            }
        }

        public async void JoinGameClick()
        {
            errorMessage.gameObject.SetActive(false);
            clientUI.SetActive(false);
            UICurtain.SetActive(true);

            var isGameJoined = await NetworkSystem.Instance.TryJoinGame(joinGameTextField.text);

            if (isGameJoined == true)
            {
                menuCanvas.SetActive(false);
            }
            else
            {
                StartCoroutine(DisplayErrorMessage(data.ErrorMessageJoiningGame));
                UICurtain.SetActive(false);
                clientUI.SetActive(true);
            }
        }

        IEnumerator DisplayErrorMessage(string message)
        {
            errorMessage.gameObject.SetActive(true);
            errorMessage.text = message;
            yield return new WaitForSeconds(5);
            errorMessage.gameObject.SetActive(false);
        }
    }
}
