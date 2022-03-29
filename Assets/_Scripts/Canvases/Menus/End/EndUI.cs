using System;
using System.Linq;
using Canvases.Components;
using Fusion;
using Managers.Game;
using Managers.Score;
using Sirenix.OdinInspector;
using Systems;
using Systems.Level;
using Systems.Network;
using Systems.Settings;
using Systems.Teams;
using Units.Player;
using UnityEngine;

namespace Canvases.Menu.End
{
    [RequireComponent(typeof(EndUINetworkData))]
    public class EndUI : AbstractMenu
    {
        [SerializeField, Required] private ButtonUIComponent replayButton;
        [SerializeField, Required] private ButtonUIComponent mainMenuButton;
        [SerializeField, Required] private TextUIComponent winnerText;
        [SerializeField, Required] private TextUIComponent scoreTeam1Text;
        [SerializeField, Required] private TextUIComponent scoreTeam2Text;
        [SerializeField, Required] private TextUIComponent nameTeam1Text;
        [SerializeField, Required] private TextUIComponent nameTeam2Text;
        [SerializeField, Required] private TextUIComponent replayReadyText;

        private EndUINetworkData networkData;
        private PlayerEntity player;
        
        private Color replayNotReadyColor = Color.black;
        
        protected override EntryDirection EnterDirection => EntryDirection.Down;
        protected override EntryDirection LeaveDirection => EntryDirection.Down;

        protected override void Awake()
        {
            base.Awake();
            networkData = GetComponent<EndUINetworkData>();

            replayNotReadyColor = replayReadyText.Color;
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            replayButton.OnClick += OnReplayPressed;
            mainMenuButton.OnClick += OnMainMenuPressed;

            if (GameManager.HasInstance)
                GameManager.Instance.OnGameStateChanged += OnGameStateChanged;
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            replayButton.OnClick -= OnReplayPressed;
            mainMenuButton.OnClick -= OnMainMenuPressed;
            
            if (GameManager.HasInstance)
                GameManager.Instance.OnGameStateChanged += OnGameStateChanged;
        }
        
        private void OnGameStateChanged(GameState gameState)
        {
            if (gameState == GameState.Finished)
            {
                Show();
            }
        }

        protected override bool ShowForImplementation(PlayerEntity playerEntity)
        {
            player = playerEntity;
            Init();
            return true;
        }

        protected override bool ShowImplementation()
        {
            player = PlayerSystem.Instance.LocalPlayer;
            Init();
            return true;
        }

        protected override bool HideImplementation()
        {
            Terminate();
            return true;
        }

        private void OnDestroy()
        {
            Terminate();
        }

        private void Init()
        {
            networkData.Reset();
            networkData.OnNumberOfPlayersReadyToReplayChanged += OnNumberOfPlayersReadyToReplayChanged;
            networkData.OnReplaying += ReturnToLobby;
            PlayerEntity.OnPlayerSpawned += OnNumberOfPlayersChanged;
            PlayerEntity.OnPlayerDespawned += OnNumberOfPlayersChanged;
            UpdateTexts();
            UpdateReadyToLeave();
        }
        
        private void Terminate()
        {
            networkData.OnNumberOfPlayersReadyToReplayChanged -= OnNumberOfPlayersReadyToReplayChanged;
            networkData.OnReplaying -= ReturnToLobby;
            PlayerEntity.OnPlayerSpawned -= OnNumberOfPlayersChanged;
            PlayerEntity.OnPlayerDespawned -= OnNumberOfPlayersChanged;
        }

        private void UpdateTexts()
        {
            if (!ScoreManager.HasInstance)
                return;
            
            UpdateWinnerText();
            UpdateTeamPointsText();
        }

        private void UpdateWinnerText()
        {
            var settings = SettingsSystem.GameSettings;
            
            if (ScoreManager.Instance.AreScoresEqual())
            {
                winnerText.Color = settings.DrawColor;
                winnerText.Text = "DRAW";
            }
            else
            {
                // Victory - Defeat.
                var teamWithHighestScore = ScoreManager.Instance.FindTeamWithHighestScore();
                var currentPlayerTeam = TeamSystem.Instance.GetTeam(player.TeamId);

                if (teamWithHighestScore == currentPlayerTeam)
                {
                    winnerText.Color = settings.VictoryColor;
                    winnerText.Text = "VICTORY!";
                }
                else
                {
                    winnerText.Color = settings.DefeatColor;
                    winnerText.Text = "DEFEAT...";
                }
            }
        }

        private void UpdateTeamPointsText()
        {
            var team1 = TeamSystem.Instance.Teams[0];
            var team2 = TeamSystem.Instance.Teams[1];

            scoreTeam1Text.Text = $"{team1.ScoreValue}";
            scoreTeam2Text.Text = $"{team2.ScoreValue}";

            nameTeam1Text.Text = team1.Name;
            nameTeam2Text.Text = team2.Name;

            var customization = SettingsSystem.CustomizationSettings;
            nameTeam1Text.Color = customization.GetColor(team1.Color);
            nameTeam2Text.Color = customization.GetColor(team2.Color);
        }

        private void OnNumberOfPlayersChanged(NetworkObject obj)
        {
            UpdateReadyToLeave();
        }

        private void OnNumberOfPlayersReadyToReplayChanged()
        {
            UpdateReadyToLeave();
        }

        private void UpdateReadyToLeave()
        {
            var settings = SettingsSystem.GameSettings;
            replayReadyText.Color = networkData.IsPlayerReadyToReplay(player.Object.InputAuthority) ? settings.ReplayReadyColor : replayNotReadyColor;
            
            var totalNumber = PlayerSystem.Instance.NumberOfPlayers;
            if (networkData.IsReplaying)
            {
                replayReadyText.Text = $"({totalNumber}/{totalNumber})";
                return;
            }

            var numberReadyToReplay = CountNumberOfPlayersReadyToReplay();
            
            replayReadyText.Text = $"({numberReadyToReplay}/{totalNumber})";

            if (numberReadyToReplay >= totalNumber)
            {
                networkData.IsReplaying = true;
            }
        }

        private int CountNumberOfPlayersReadyToReplay()
        {
            return PlayerSystem.Instance.AllPlayers.Count(playerEntity => networkData.IsPlayerReadyToReplay(playerEntity.Object.InputAuthority));
        }
        
        private void OnReplayPressed()
        {
            networkData.LocalToggleReadyToReplay(player.Object.InputAuthority);
        }

        private void OnMainMenuPressed()
        {
            LevelSystem.Instance.LoadMainMenu();
            NetworkSystem.Instance.Disconnect();
        }
        
        private void ReturnToLobby()
        {
            if (GameManager.HasInstance)
            {
                GameManager.Instance.CleanUpAndReturnToLobby();
            }
        }
    }
}