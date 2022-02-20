using Fusion;

namespace Systems.Network
{
    public partial class NetworkSystem
    {
        public static float DeltaTime => Instance.networkRunner ? Instance.networkRunner.DeltaTime : 0f;
        
        public bool IsConnected => networkRunner != null;

        public bool IsHost => IsConnected && networkRunner.GameMode == GameMode.Host;
        public bool IsClient => IsConnected && networkRunner.GameMode == GameMode.Client;
    }
}