using System;
using Canvases.Components;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Canvases.Customization
{
    public abstract class CustomizationUIButton : CustomizationUIElement
    {
        [SerializeField, Required] private ButtonUIComponent button;

        private void OnEnable()
        {
            button.OnClick += OnClick;
        }

        private void OnDisable()
        {
            button.OnClick -= OnClick;
        }

        protected abstract void OnClick();
    }
}