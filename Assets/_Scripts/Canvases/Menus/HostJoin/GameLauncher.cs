using Canvases.Menu;
using Systems.Network;
using Systems.Settings;
using TMPro;
using UnityEngine;

namespace Canvases.Matchmaking
{
    public class GameLauncher : MonoBehaviour
    {

        private MatchmakingSettings data;

        [SerializeField] private GameObject hostUI;
        [SerializeField] private GameObject clientUI;
        // ReSharper disable once InconsistentNaming Reason: UI should be capitalized
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
            
            if (isGameCreated)
            {
                menuCanvas.SetActive(false);
            }
            else
            {
                if (MenuManager.HasInstance)
                {
                    MenuManager.Instance.ShowModal(data.ErrorMessageCreatingGame, data.ErrorMessageHeader);
                }
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

            if (isGameJoined)
            {
                menuCanvas.SetActive(false);
            }
            else
            {
                if (MenuManager.HasInstance)
                {
                    MenuManager.Instance.ShowModal(data.ErrorMessageJoiningGame, data.ErrorMessageHeader);
                }
                UICurtain.SetActive(false);
                clientUI.SetActive(true);
            }
        }
    }
}
