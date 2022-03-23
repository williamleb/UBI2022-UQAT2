using Sirenix.OdinInspector;
using Systems.Camera;
using Units.Camera;
using UnityEngine;

namespace Units.Player
{
    public partial class PlayerEntity
    {
        [Header("Camera")]
        [SerializeField, Required] private CameraStrategy cameraStrategy;
        [SerializeField, Required] private VirtualCamera mainCamera;
        [SerializeField, Required] private VirtualCamera customizationCamera;

        private void InitCamera()
        {
            if (!Object.HasInputAuthority)
                return;
            
            cameraStrategy.Init(data.PlayerCameraSetting);
            mainCamera.Activate();
        }
        
    }
}