using UnityEngine;

namespace Scriptables
{
    [CreateAssetMenu(fileName = "GamepadIcons_", menuName = "AssetPack/GamePad Icons", order = 0)]
    public class GamepadIcons : ScriptableObject
    {
        public Sprite ButtonSouth;
        public Sprite ButtonNorth;
        public Sprite ButtonEast;
        public Sprite ButtonWest;
        public Sprite StartButton;
        public Sprite SelectButton;
        public Sprite LeftTrigger;
        public Sprite RightTrigger;
        public Sprite LeftShoulder;
        public Sprite RightShoulder;
        public Sprite Dpad;
        public Sprite DpadUp;
        public Sprite DpadDown;
        public Sprite DpadLeft;
        public Sprite DpadRight;
        public Sprite LeftStick;
        public Sprite RightStick;
        public Sprite LeftStickPress;
        public Sprite RightStickPress;

        public Sprite GetSprite(string controlPath)
        {
            // From the input system, we get the path of the control on device. So we can just
            // map from that to the sprites we have for gamepads.
            return controlPath switch
            {
                "buttonSouth" => ButtonSouth,
                "buttonNorth" => ButtonNorth,
                "buttonEast" => ButtonEast,
                "buttonWest" => ButtonWest,
                "start" => StartButton,
                "select" => SelectButton,
                "leftTrigger" => LeftTrigger,
                "rightTrigger" => RightTrigger,
                "leftShoulder" => LeftShoulder,
                "rightShoulder" => RightShoulder,
                "dpad" => Dpad,
                "dpad/up" => DpadUp,
                "dpad/down" => DpadDown,
                "dpad/left" => DpadLeft,
                "dpad/right" => DpadRight,
                "leftStick" => LeftStick,
                "rightStick" => RightStick,
                "leftStickPress" => LeftStickPress,
                "rightStickPress" => RightStickPress,
                _ => null
            };
        }
    }
}