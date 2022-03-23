using Canvases.Components;
using UnityEngine;

namespace Canvases.Menu.Customization
{
    public class CustomizationUISkin : CustomizationUIUpDown
    {
        [SerializeField] private TextUIComponent customizationNumber;

        protected override void Init()
        {
            base.Init();
            UpdateSkinNumber(Customization.Skin);
            Customization.OnSkinChangedEvent += UpdateSkinNumber;
        }

        protected override void Terminate()
        {
            base.Terminate();
            Customization.OnSkinChangedEvent -= UpdateSkinNumber;
        }

        protected override void OnUp()
        {
            Customization.IncrementSkin();
        }

        protected override void OnDown()
        {
            Customization.DecrementSkin();
        }

        private void UpdateSkinNumber(int skinNumber)
        {
            customizationNumber.Text = $"{skinNumber + 1}";
        }
    }
}