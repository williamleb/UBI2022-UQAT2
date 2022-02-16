using UnityEngine;

namespace Scriptables
{
    [CreateAssetMenu(fileName = "GamepadIcons_", menuName = "AssetPack/GamePad Icons", order = 0)]
    public class GamepadIcons : ScriptableObject
    {
        public Sprite buttonSouth;
        public Sprite buttonNorth;
        public Sprite buttonEast;
        public Sprite buttonWest;
        public Sprite startButton;
        public Sprite selectButton;
        public Sprite leftTrigger;
        public Sprite rightTrigger;
        public Sprite leftShoulder;
        public Sprite rightShoulder;
        public Sprite dpad;
        public Sprite dpadUp;
        public Sprite dpadDown;
        public Sprite dpadLeft;
        public Sprite dpadRight;
        public Sprite leftStick;
        public Sprite rightStick;
        public Sprite leftStickPress;
        public Sprite rightStickPress;

        public Sprite GetSprite(string controlPath)
        {
            // From the input system, we get the path of the control on device. So we can just
            // map from that to the sprites we have for gamepads.
            return controlPath switch
            {
                "buttonSouth" => buttonSouth,
                "buttonNorth" => buttonNorth,
                "buttonEast" => buttonEast,
                "buttonWest" => buttonWest,
                "start" => startButton,
                "select" => selectButton,
                "leftTrigger" => leftTrigger,
                "rightTrigger" => rightTrigger,
                "leftShoulder" => leftShoulder,
                "rightShoulder" => rightShoulder,
                "dpad" => dpad,
                "dpad/up" => dpadUp,
                "dpad/down" => dpadDown,
                "dpad/left" => dpadLeft,
                "dpad/right" => dpadRight,
                "leftStick" => leftStick,
                "rightStick" => rightStick,
                "leftStickPress" => leftStickPress,
                "rightStickPress" => rightStickPress,
                _ => null
            };
        }
    }
}