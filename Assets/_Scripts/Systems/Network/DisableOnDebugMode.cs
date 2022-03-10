using UnityEngine;

namespace Systems.Network
{
    public class DisableOnDebugMode : MonoBehaviour
    {
        [SerializeField] private bool invert;

        private void Awake()
        {
            gameObject.SetActive(NetworkSystem.Instance.DebugMode == invert);
        }
    }
}