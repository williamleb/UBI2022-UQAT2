using Scriptables;
using UnityEngine;

namespace Utilities
{
    public static class GamepadIconsUtil
    {
        private static GamepadIcons xbox;
        private static GamepadIcons ps4;

        private static void Init()
        {
            xbox = Resources.Load<GamepadIcons>("InputSystem/GamePadIcons_Xbox");
            ps4 = Resources.Load<GamepadIcons>("InputSystem/GamePadIcons_PS");
        }

        public static Sprite OnUpdateBindingDisplay(string deviceLayoutName, string mainControlPath)
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

            return icon;
        }
    }
}