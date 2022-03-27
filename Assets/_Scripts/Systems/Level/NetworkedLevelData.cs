using Fusion;

namespace Systems.Level
{
    public class NetworkedGameData : NetworkBehaviour
    {
        [Networked] public int PhaseTotalHomework { get; set; }
    }
}