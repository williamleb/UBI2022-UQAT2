using Canvases.Components;
using UnityEngine;

namespace Canvases.Menu.Customization
{
    public class CustomizationUIHairColor : CustomizationUIUpDown
    {
        [SerializeField] private TextUIComponent customizationNumber;

        protected override void Init()
        {
            base.Init();
            UpdateHairColorNumber(Customization.HairColor);
            Customization.OnHairColorChangedEvent += UpdateHairColorNumber;
        }

        protected override void Terminate()
        {
            base.Terminate();
            Customization.OnHairColorChangedEvent -= UpdateHairColorNumber;
        }

        protected override void OnUp()
        {
            Customization.IncrementHairColor();
        }

        protected override void OnDown()
        {
            Customization.DecrementHairColor();
        }

        private void UpdateHairColorNumber(int hairColorNumber)
        {
            customizationNumber.Text = $"{hairColorNumber + 1}";
        }
    }
}