namespace Canvases.Customization
{
    public class CustomizationUIRandomize : CustomizationUIButton
    {
        protected override void OnClick()
        {
            CustomizablePlayer.Customization.Randomize();
        }
    }
}