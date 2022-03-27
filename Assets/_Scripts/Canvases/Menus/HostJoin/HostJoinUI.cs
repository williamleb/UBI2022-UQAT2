using System.Collections;
using Canvases.Animations;
using Canvases.Components;
using Canvases.Menu;
using Sirenix.OdinInspector;
using Systems;
using Systems.Network;
using Systems.Settings;
using UnityEngine;

namespace Canvases.Matchmaking
{
    [RequireComponent(typeof(CanvasGroup))]
    public class HostJoinUI : AbstractMenu
    {
        [SerializeField, Required] private FadeAnimation connectionCurtain;
        [SerializeField, Required] private ButtonUIComponent hostOrJoinButton;
        [SerializeField, Required] private ButtonUIComponent backButton;
        [SerializeField, Required] private HostJoinSequence sequence;
        [SerializeField] private CanvasGroup menuCanvasGroup;
        [SerializeField] private bool host = true;

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
            if (NetworkSystem.Instance.IsGameStartedOrStarting)
                return;
            
            connectionCurtain.FadeIn();
           
            var isGameCreated = await NetworkSystem.Instance.CreateGame(sequence.Value);

            if (isGameCreated)
            {
                HideCanvas();
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
            if (NetworkSystem.Instance.IsGameStartedOrStarting)
                return;
            
            connectionCurtain.FadeIn();

            var isGameJoined = await NetworkSystem.Instance.TryJoinGame(sequence.Value);

            if (isGameJoined)
            {
                HideCanvas();
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
            if (!MenuManager.HasInstance)
                return;
            
            MenuManager.Instance.Close();
        }

        public override void OnMenuManagerClosed()
        {
            menuCanvasGroup.alpha = 0f;
            menuCanvasGroup.interactable = false;
            StartCoroutine(WaitUntilConnectionIsHiddenToHide());
        }

        public override void OnMenuManagerOpened()
        {
            base.OnMenuManagerOpened();
            menuCanvasGroup.alpha = 1f;
            menuCanvasGroup.interactable = true;
        }

        private IEnumerator WaitUntilConnectionIsHiddenToHide()
        {
            yield return new WaitUntil(() => LevelSystem.Instance.State == LevelSystem.LevelState.Lobby);
            connectionCurtain.FadeOut();
            yield return new WaitUntil(() => connectionCurtain.IsFadedOut);
            base.OnMenuManagerClosed();
        }
    }
}
