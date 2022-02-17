using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.InputSystem;

namespace InputSystem
{
    public class DetectDevice
    {
        public event Action<string> OnInputDeviceChanged;

        private readonly Dictionary<string, string> deviceToControlScheme = new Dictionary<string, string>();

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
        }

        public string CurrentDevice { get; private set; }

        private void OnAnyInput(InputAction.CallbackContext obj)
        {
            if (obj.control.device.name == "Mouse") return;

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