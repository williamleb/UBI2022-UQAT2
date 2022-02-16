﻿using System;
using Canvases.Components;
using UnityEngine;

namespace Canvases.InputSystem
{
    [Serializable]
    public class RebindReferences
    {
        public TextUIComponent Text;
        public ButtonUIComponent Button;
        public ImageUIComponent Image;
        [HideInInspector] public int Index = -1;
    }
}