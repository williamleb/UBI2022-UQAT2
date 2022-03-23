using Canvases.Components;
using UnityEngine;

namespace Canvases.Menu.Customization
{
    public class CustomizationUIHead : CustomizationUIUpDown
    {
        [SerializeField] private TextUIComponent customizationNumber;

        protected override void Init()
        {
            base.Init();
            UpdateHeadNumber(Customization.Head);
            Customization.OnHeadChangedEvent += UpdateHeadNumber;
        }

        protected override void Terminate()
        {
            base.Terminate();
            Customization.OnHeadChangedEvent -= UpdateHeadNumber;
        }

        protected override void OnUp()
        {
            Customization.IncrementHead();
        }

        protected override void OnDown()
        {
            Customization.DecrementHead();
        }

        private void UpdateHeadNumber(int headNumber)
        {
            customizationNumber.Text = $"{headNumber + 1}";
        }
    }
}