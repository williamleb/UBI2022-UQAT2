using Canvases.Components;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Canvases.Menu.Customization
{
    public abstract class CustomizationUIUpDown : CustomizationUIElement
    {
        [SerializeField, Required] private ButtonUIComponent up;
        [SerializeField, Required] private ButtonUIComponent down;

        private void OnEnable()
        {
            up.OnClick += OnUp;
            down.OnClick += OnDown;
        }

        private void OnDisable()
        {
            up.OnClick -= OnUp;
            down.OnClick -= OnDown;
        }

        protected abstract void OnUp();
        protected abstract void OnDown();
    }
}