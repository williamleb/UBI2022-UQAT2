using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

namespace InputSystem
{
    public class DetectDevice
    {
        private const float THRESHOLD_TO_DETECT_DEVICE_FROM_MOVEMENT = 0.2f;
        
        public event Action<string> OnInputDeviceChanged;

        private readonly Dictionary<string, string> deviceToControlScheme = new Dictionary<string, string>();

        private readonly InputActionMap actionMap;

        public DetectDevice(InputActionMap inputActionMap)
        {
            foreach (InputControlScheme inputControlScheme in inputActionMap.controlSchemes)
            {
                foreach (InputControlScheme.DeviceRequirement deviceRequirement in
                         inputControlScheme.deviceRequirements)
                {
                    deviceToControlScheme.Add(deviceRequirement.controlPath, inputControlScheme.name);
                }
            }

            CurrentDevice = deviceToControlScheme.Values.First();
            inputActionMap.actionTriggered += OnAnyInput;
            actionMap = inputActionMap;
        }

        public void FinalizeInit()
        {
            if (actionMap == null)
                return;

            actionMap.actionTriggered -= OnAnyInput;
        }

        public string CurrentDevice { get; private set; }

        private void OnAnyInput(InputAction.CallbackContext obj)
        {
            if (obj.control.device.name == "Mouse") return;

            // We don't want to detect a new device from small movement (controllers can often drift)
            if (obj.action.name == "Movement" && obj.valueType == typeof(Vector2))
            {
                var value = obj.ReadValue<Vector2>();
                if (value.sqrMagnitude < THRESHOLD_TO_DETECT_DEVICE_FROM_MOVEMENT)
                    return;
            }
            
            foreach (string key in deviceToControlScheme.Keys)
            {
                if (obj.control.device.name.Contains(key.Substring(1, key.Length - 2)))
                {
                    string detectedDevice = deviceToControlScheme[key];
                    if (detectedDevice != CurrentDevice)
                    {
                        CurrentDevice = detectedDevice;
                        OnInputDeviceChanged?.Invoke(CurrentDevice);
                    }

                    return;
                }
            }
        }
    }
}