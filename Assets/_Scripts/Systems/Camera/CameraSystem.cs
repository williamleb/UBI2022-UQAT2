using System.Collections.Generic;
using Cinemachine;
using UnityEngine;
using Utilities.Singleton;

namespace Systems.Camera
{
    public class CameraSystem : PersistentSingleton<CameraSystem>
    {
        private const int ACTIVATED_PRIORITY = 100;
        private const int DEACTIVATED_PRIORITY = 10;
        
        private UnityEngine.Camera mainCamera;
        private List<VirtualCamera> virtualCameras = new List<VirtualCamera>();

        public UnityEngine.Camera MainCamera => mainCamera;
        public IEnumerable<VirtualCamera> VirtualCameras => virtualCameras;

        protected override void Awake()
        {
            base.Awake();
            InitializeMainCamera();
        }
        
        private void OnEnable()
        {
            LevelSystem.Instance.OnMainMenuLoad += OnReturnToMainMenu;
        }

        private void OnDisable()
        {
            if (LevelSystem.HasInstance)
            {
                LevelSystem.Instance.OnMainMenuLoad -= OnReturnToMainMenu;
            }
        }

        private void OnReturnToMainMenu()
        {
            if (mainCamera)
            {
                Destroy(mainCamera);
                mainCamera = null;
            }
            InitializeMainCamera();
        }

        public void RegisterVirtualCamera(VirtualCamera virtualCamera)
        {
            virtualCameras.Add(virtualCamera);
        }

        public void UnregisterVirtualCamera(VirtualCamera virtualCamera)
        {
            virtualCameras.Remove(virtualCamera);
        }

        public void ActivateVirtualCamera(VirtualCamera virtualCamera)
        {
            DeactivateAllCameras();
            virtualCamera.m_Priority = ACTIVATED_PRIORITY;
        }

        private void DeactivateAllCameras()
        {
            foreach (var virtualCamera in virtualCameras)
            {
                virtualCamera.Priority = DEACTIVATED_PRIORITY;
            }
        }

        private void InitializeMainCamera()
        {
            mainCamera = UnityEngine.Camera.main;
            Debug.Assert(mainCamera, "Every scene needs a main camera");
            if (!mainCamera)
                return;
            
            Debug.Assert(mainCamera.GetComponent<CinemachineBrain>(), "Main camera must have a cinemachine brain");
            DontDestroyOnLoad(mainCamera.gameObject);
        }
    }
}