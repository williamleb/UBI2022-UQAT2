using UnityEngine;

namespace Systems.Network
{
    public class DeactivateDebugMode : MonoBehaviour
    {
        void Start()
        {
            NetworkSystem.Instance.DebugMode = false;
        }
    }
}