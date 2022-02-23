using System;
using InputSystem;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Canvases.Prompts
{
    [RequireComponent(typeof(PlayerInput))]
    class Prompt : MonoBehaviour
    {
        private PlayerInput playerInput;
        private DetectDevice detectDevice;

        private void Awake()
        {
            playerInput = playerInput;
        }

        private void Start()
        {
            detectDevice = new DetectDevice(playerInput.currentActionMap);
        }

        private void OnEnable()
        {
            detectDevice.OnInputDeviceChanged += OnInputDeviceChanged;
        }

        private void OnDisable()
        {
            detectDevice.OnInputDeviceChanged -= OnInputDeviceChanged;
        }

        private void OnInputDeviceChanged(string device)
        {
            
        }
    }
}