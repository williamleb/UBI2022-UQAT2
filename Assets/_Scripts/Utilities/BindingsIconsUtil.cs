using System.Collections.Generic;
using System.Linq;
using Scriptables;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Utilities
{
    public static class BindingsIconsUtil
    {
        private static GamepadIcons xbox;
        private static GamepadIcons ps4;
        private static MouseKeyboardIcons mouseKeyboard;

        private static void Init()
        {
            xbox = Resources.Load<GamepadIcons>("InputSystem/GamePadIcons_Xbox");
            ps4 = Resources.Load<GamepadIcons>("InputSystem/GamePadIcons_PS");
            mouseKeyboard = Resources.Load<MouseKeyboardIcons>("InputSystem/MouseKeyboardIcons");
        }

        public static Sprite GetSprite(string deviceLayoutName, string mainControlPath)
        {
            if (xbox == null) Init();

            if (string.IsNullOrEmpty(deviceLayoutName) || string.IsNullOrEmpty(mainControlPath))
                return null;

            Sprite icon = default;
            if (UnityEngine.InputSystem.InputSystem.IsFirstLayoutBasedOnSecond(deviceLayoutName, "DualShockGamepad"))
            {
                icon = ps4.GetSprite(mainControlPath);
            }
            else if (UnityEngine.InputSystem.InputSystem.IsFirstLayoutBasedOnSecond(deviceLayoutName, "Gamepad"))
            {
                icon = xbox.GetSprite(mainControlPath);
            }
            else if (UnityEngine.InputSystem.InputSystem.IsFirstLayoutBasedOnSecond(deviceLayoutName, "Keyboard"))
            {
                icon = mouseKeyboard.GetSprite(mainControlPath);
            }

            return icon;
        }
        
        public static Sprite GetSprite(InputAction inputAction, string deviceName)
        {
            if (inputAction == null || string.IsNullOrEmpty(deviceName))
                return null;
            
            var mainBindings = GetRelevantMainBindings(inputAction, deviceName);
            if (!mainBindings.Any())
                return null;

            var mainBinding = mainBindings.First();

            inputAction.GetBindingDisplayString(mainBinding, out var deviceLayoutName, out var mainControlPath);
            
            return GetSprite(deviceLayoutName, mainControlPath);
        }

        private static readonly List<int> relevantMainBindings = new List<int>(); // So we don't create an instance each time the next method is called
        public static List<int> GetRelevantMainBindings(InputAction inputAction, string deviceName)
        {
            relevantMainBindings.Clear();
            
            if (inputAction.bindings[0].isComposite)
            {
                if (deviceName == "Gamepad")
                {
                    relevantMainBindings.Add(inputAction.bindings.Count - 2);
                }
                else
                {
                    for (int i = 1; i < inputAction.bindings.Count - 2; i += 2)
                    {
                        relevantMainBindings.Add(i);
                    }
                }
            }
            else
            {
                relevantMainBindings.Add(deviceName == "Gamepad" ? 2 : 0);
            }

            return relevantMainBindings;
        }
    }
}