using Cinemachine;

namespace Systems.Camera
{
    public class VirtualCamera : CinemachineVirtualCamera
    {
        protected override void Start()
        {
            base.Start();
            if (CameraSystem.HasInstance)
            {
                CameraSystem.Instance.RegisterVirtualCamera(this);
            }
        }

        protected override void OnDestroy()
        {
            if (CameraSystem.HasInstance)
            {
                CameraSystem.Instance.UnregisterVirtualCamera(this);
            }
            
            base.OnDestroy();
        }

        public void Activate()
        {
            CameraSystem.Instance.ActivateVirtualCamera(this);
        }
    }
}