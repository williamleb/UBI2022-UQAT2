using UnityEngine;

namespace Scriptables
{
    [CreateAssetMenu(fileName = "MouseKeyboardIcons_", menuName = "AssetPack/MouseKeyboard Icons", order = 0)]
    public class MouseKeyboardIcons : ScriptableObject
    {
        public Sprite Zero;
        public Sprite One;
        public Sprite Two;
        public Sprite Three;
        public Sprite Four;
        public Sprite Five;
        public Sprite Six;
        public Sprite Seven;
        public Sprite Eight;
        public Sprite Nine;
        public Sprite Alt;
        public Sprite ArrowDown;
        public Sprite ArrowLeft;
        public Sprite ArrowRight;
        public Sprite ArrowUp;
        public Sprite Asterisk;
        public Sprite Backspace;
        public Sprite Ctrl;
        public Sprite Delete;
        public Sprite End;
        public Sprite Enter;
        public Sprite Escape;
        public Sprite F1;
        public Sprite F2;
        public Sprite F3;
        public Sprite F4;
        public Sprite F5;
        public Sprite F6;
        public Sprite F7;
        public Sprite F8;
        public Sprite F9;
        public Sprite F10;
        public Sprite F11;
        public Sprite F12;
        public Sprite Home;
        public Sprite LetterA;
        public Sprite LetterB;
        public Sprite LetterC;
        public Sprite LetterD;
        public Sprite LetterE;
        public Sprite LetterF;
        public Sprite LetterG;
        public Sprite LetterH;
        public Sprite LetterI;
        public Sprite LetterJ;
        public Sprite LetterK;
        public Sprite LetterL;
        public Sprite LetterM;
        public Sprite LetterN;
        public Sprite LetterO;
        public Sprite LetterP;
        public Sprite LetterQ;
        public Sprite LetterR;
        public Sprite LetterS;
        public Sprite LetterT;
        public Sprite LetterU;
        public Sprite LetterV;
        public Sprite LetterW;
        public Sprite LetterX;
        public Sprite LetterY;
        public Sprite LetterZ;
        public Sprite MarkLeft;
        public Sprite MarkRight;
        public Sprite Minus;
        public Sprite Plus;
        public Sprite MouseLeft;
        public Sprite MouseMiddle;
        public Sprite MouseRight;
        public Sprite MouseNone;
        public Sprite PageDown;
        public Sprite PageUp;
        public Sprite QuestionMark;
        public Sprite Quote;
        public Sprite Semicolon;
        public Sprite Shift;
        public Sprite Slash;
        public Sprite Space;
        public Sprite Tab;
        public Sprite Tilda;

        public Sprite GetSprite(string controlPath)
        {
            // From the input system, we get the path of the control on device. So we can just
            // map from that to the sprites we have for gamepads.
            return controlPath.ToLower() switch
            {
                "0" => Zero,
                "1" => One,
                "2" => Two,
                "3" => Three,
                "4" => Four,
                "5" => Five,
                "6" => Six,
                "7" => Seven,
                "8" => Eight,
                "9" => Nine,
                "alt" => Alt,
                "downarrow" => ArrowDown,
                "leftarrow" => ArrowLeft,
                "rightarrow" => ArrowRight,
                "uparrow" => ArrowUp,
                "*" => Asterisk,
                "backspace" => Backspace,
                "ctrl" => Ctrl,
                "delete" => Delete,
                "end" => End,
                "enter" => Enter,
                "escape" => Escape,
                "f1" => F1,
                "f2" => F2,
                "f3" => F3,
                "f4" => F4,
                "f5" => F5,
                "f6" => F6,
                "f7" => F7,
                "f8" => F8,
                "f9" => F9,
                "f10" => F10,
                "f11" => F11,
                "f12" => F12,
                "home" => Home,
                "a" => LetterA,
                "b" => LetterB,
                "c" => LetterC,
                "d" => LetterD,
                "e" => LetterE,
                "f" => LetterF,
                "g" => LetterG,
                "h" => LetterH,
                "i" => LetterI,
                "j" => LetterJ,
                "k" => LetterK,
                "l" => LetterL,
                "m" => LetterM,
                "n" => LetterN,
                "o" => LetterO,
                "p" => LetterP,
                "q" => LetterQ,
                "r" => LetterR,
                "s" => LetterS,
                "t" => LetterT,
                "u" => LetterU,
                "v" => LetterV,
                "w" => LetterW,
                "x" => LetterX,
                "y" => LetterY,
                "z" => LetterZ,
                "[" => MarkLeft,
                "]" => MarkRight,
                "-" => Minus,
                "+" => Plus,
                "leftmouse" => MouseLeft,
                "middlemouse" => MouseMiddle,
                "rightmouse" => MouseRight,
                "press" => MouseNone,
                "pagedown" => PageDown,
                "pageup" => PageUp,
                "?" => QuestionMark,
                "\"" => Quote,
                ";" => Semicolon,
                "shift" => Shift,
                "/" => Slash,
                "space" => Space,
                "tab" => Tab,
                "~" => Tilda,
                _ => null
            };
        }
    }
}