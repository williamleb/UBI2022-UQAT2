using Sirenix.OdinInspector;
using UnityEngine;

namespace Systems.Settings
{
    [CreateAssetMenu(menuName = "Settings/Network Settings")]
    public class NetworkSettings : ScriptableObject
    {
        [Header("Network settings")]
        [SerializeField] private string lobbyName = "bababooeyLobby";
        [SerializeField] private string hostConnectionLostHeader = "Lost connection";
        [SerializeField] private string hostConnectionLostMessage = "The connection with the host has been lost.";

        [Header("Matchmaking settings")]
        [Tooltip("Allow to start a single player game when only one player is in the lobby and ready")]
        [SerializeField]
        private bool allowSoloPlay = true;

        [SerializeField]
        private string errorMessageHeader = "Erreur";
        
        [Tooltip("Error message when a player tries to connect to a game but it does not exist")]
        [SerializeField]
        private string errorMessageJoiningGame = "Erreur lors de la connection à la partie.";

        [Tooltip("Error message when a player tries to create a game but fails")]
        [SerializeField]
        private string errorMessageCreatingGame = "Erreur lors de la création d'une partie.";

        [Header("Lobby settings")]
        [Tooltip("Countdown to the start of the game when all players are ready")] [SerializeField, MinValue(1)] 
        private int countDownTime = 5;

        [Tooltip("Countdown message when all players are ready showing the remaining time. Ex : \"Game starts in 10...\"")]
        [SerializeField]
        private string countDownMessage = "La partie débute dans";

        [Header("Transition settings")] 
        [SerializeField] private string hostToLobbyMessage = "CREATING GAME";
        [SerializeField] private string clientToLobbyMessage = "JOINING GAME";
        [SerializeField] private string gameToLobbyMessage = "RETURNING TO LOBBY";
        [SerializeField] private string lobbyToGameMessage = "LOADING GAME";

        public bool AllowSoloPlay => allowSoloPlay;
        public string ErrorMessageHeader => errorMessageHeader;
        public string ErrorMessageJoiningGame => errorMessageJoiningGame;
        public string ErrorMessageCreatingGame => errorMessageCreatingGame;
        public int CountDownTime => countDownTime;
        public string CountDownMessage => countDownMessage;
        public string LobbyName => lobbyName;
        public string HostConnectionLostHeader => hostConnectionLostHeader;
        public string HostConnectionLostMessage => hostConnectionLostMessage;
        public string HostToLobbyMessage => hostToLobbyMessage;
        public string ClientToLobbyMessage => clientToLobbyMessage;
        public string GameToLobbyMessage => gameToLobbyMessage;
        public string LobbyToGameMessage => lobbyToGameMessage;
    }
}