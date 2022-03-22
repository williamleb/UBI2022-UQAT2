namespace Canvases.Menu.Customization
{
    public class CustomizationUIRandomize : CustomizationUIButton
    {
        protected override void OnClick()
        {
            Customization.Randomize();
        }
    }
}