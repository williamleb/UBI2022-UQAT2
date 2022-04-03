using Canvases.Components;
using Canvases.Menu;
using Canvases.TransitionScreen;
using Sirenix.OdinInspector;
using Systems.Network;
using Systems.Settings;
using UnityEngine;

namespace Canvases.Matchmaking
{
    [RequireComponent(typeof(CanvasGroup))]
    public class HostJoinUI : AbstractMenu
    {
        [SerializeField, Required] private ButtonUIComponent hostOrJoinButton;
        [SerializeField, Required] private ButtonUIComponent backButton;
        [SerializeField, Required] private HostJoinSequence sequence;
        [SerializeField] private bool host = true;

        protected override EntryDirection EnterDirection => EntryDirection.Up;
        protected override EntryDirection LeaveDirection => EntryDirection.Up;
        
        private NetworkSettings data;

        public bool Host => host;

        void Start()
        {
            data = SettingsSystem.NetworkSettings;
            hostOrJoinButton.Enabled = false;
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            hostOrJoinButton.OnClick += OnHostJoinButtonPressed;
            backButton.OnClick += OnBackButtonPressed;
            sequence.OnChanged += OnSequenceChanged;
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            hostOrJoinButton.OnClick -= OnHostJoinButtonPressed;
            backButton.OnClick -= OnBackButtonPressed;
            sequence.OnChanged -= OnSequenceChanged;
        }

        private void OnSequenceChanged()
        {
            if (sequence.IsComplete)
            {
                hostOrJoinButton.Enabled = true;
                hostOrJoinButton.Select();
            }
            else
            {
                hostOrJoinButton.Enabled = false;
            }
        }

        private void OnHostJoinButtonPressed()
        {
            if (host)
                CreateGameClick();
            else
                JoinGameClick();
        }
        
        private void OnBackButtonPressed()
        {
            if (MenuManager.HasInstance)
            {
                Hide();
                MenuManager.Instance.ShowMenu(MenuManager.Menu.Main);
            }
        }

        private async void CreateGameClick()
        {
            if (NetworkSystem.Instance.IsGameStartedOrStarting)
                return;
            
            TransitionScreenSystem.Instance.Show(data.HostToLobbyMessage);
           
            var isGameCreated = await NetworkSystem.Instance.CreateGame(sequence.Value);

            if (isGameCreated)
            {
                HideCanvas();
            }
            else
            {
                if (MenuManager.HasInstance)
                {
                    Unfocus();
                    MenuManager.Instance.ShowModal(data.ErrorMessageCreatingGame, data.ErrorMessageHeader);
                }
                
                TransitionScreenSystem.Instance.Hide();
            }
        }

        private async void JoinGameClick()
        {
            if (NetworkSystem.Instance.IsGameStartedOrStarting)
                return;
            
            TransitionScreenSystem.Instance.Show(data.ClientToLobbyMessage);

            var isGameJoined = await NetworkSystem.Instance.TryJoinGame(sequence.Value);

            if (isGameJoined)
            {
                HideCanvas();
            }
            else
            {
                if (MenuManager.HasInstance)
                {
                    Unfocus();
                    MenuManager.Instance.ShowModal(data.ErrorMessageJoiningGame, data.ErrorMessageHeader);
                }
                
                TransitionScreenSystem.Instance.Hide();
            }
        }

        private void HideCanvas()
        {
            if (!MenuManager.HasInstance)
                return;
            
            MenuManager.Instance.Close();
        }
    }
}
