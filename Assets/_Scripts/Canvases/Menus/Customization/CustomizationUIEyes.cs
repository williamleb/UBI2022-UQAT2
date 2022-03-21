using Canvases.Components;
using UnityEngine;

namespace Canvases.Menu.Customization
{
    public class CustomizationUIEyes : CustomizationUIUpDown
    {
        [SerializeField] private TextUIComponent customizationNumber;

        protected override void Init()
        {
            base.Init();
            UpdateEyesNumber(Customization.Eyes);
            Customization.OnEyesChangedEvent += UpdateEyesNumber;
        }

        protected override void Terminate()
        {
            base.Terminate();
            Customization.OnEyesChangedEvent -= UpdateEyesNumber;
        }

        protected override void OnUp()
        {
            Customization.IncrementEyes();
        }

        protected override void OnDown()
        {
            Customization.DecrementEyes();
        }

        private void UpdateEyesNumber(int eyesNumber)
        {
            customizationNumber.Text = $"{eyesNumber + 1}";
        }
    }
}