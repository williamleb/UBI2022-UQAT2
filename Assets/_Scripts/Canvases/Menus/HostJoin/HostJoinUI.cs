using Canvases.Animations;
using Canvases.Components;
using Canvases.Menu;
using Sirenix.OdinInspector;
using Systems.Network;
using Systems.Settings;
using TMPro;
using UnityEngine;

namespace Canvases.Matchmaking
{
    public class HostJoinUI : AbstractMenu
    {
        [SerializeField, Required] private FadeAnimation connectionCurtain;
        [SerializeField, Required] private ButtonUIComponent hostOrJoinButton;
        [SerializeField, Required] private ButtonUIComponent backButton;
        [SerializeField, Required] private HostJoinSequence sequence;
        [SerializeField] private bool host = true;

        [SerializeField] private CanvasGroup canvasToHideWhenConnected;

        protected override EntryDirection EnterDirection => EntryDirection.Up;
        protected override EntryDirection LeaveDirection => EntryDirection.Up;
        
        private MatchmakingSettings data;

        public bool Host => host;

        void Start()
        {
            data = SettingsSystem.MatchmakingSettings;
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
            connectionCurtain.FadeIn();
           
            var isGameCreated = await NetworkSystem.Instance.CreateGame(sequence.Value);

            if (isGameCreated)
            {
                HideCanvas();
                connectionCurtain.FadeOut();
            }
            else
            {
                if (MenuManager.HasInstance)
                    MenuManager.Instance.ShowModal(data.ErrorMessageCreatingGame, data.ErrorMessageHeader);
                
                connectionCurtain.FadeOut();
            }
        }

        private async void JoinGameClick()
        {
            connectionCurtain.FadeIn();

            var isGameJoined = await NetworkSystem.Instance.TryJoinGame(sequence.Value);

            if (isGameJoined)
            {
                HideCanvas();
                connectionCurtain.FadeOut();
            }
            else
            {
                if (MenuManager.HasInstance)
                    MenuManager.Instance.ShowModal(data.ErrorMessageJoiningGame, data.ErrorMessageHeader);
                
                connectionCurtain.FadeOut();
            }
        }

        private void HideCanvas()
        {
            if (!canvasToHideWhenConnected)
                return;
            
            canvasToHideWhenConnected.alpha = 0f;
            canvasToHideWhenConnected.interactable = false;
        }
    }
}
