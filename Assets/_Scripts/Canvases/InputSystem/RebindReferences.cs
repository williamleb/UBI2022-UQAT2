using System;
using Canvases.Components;
using UnityEngine;

namespace Canvases.InputSystem
{
    [Serializable]
    public class RebindReferences
    {
        public TextUIComponent text;
        public ButtonUIComponent button;
        public ImageUIComponent image;
        [HideInInspector] public int index = -1;
    }
}