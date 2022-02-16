using UnityEngine;
using UnityEngine.UI;

namespace Canvases.Components
{
    public class ImageUIComponent : UIComponentBase
    {
        [SerializeField] private Image image;

        public Sprite Sprite
        {
            set => image.sprite = value;
        }

        public Color Color
        {
            set => image.color = value;
        }

        private void OnValidate() => image = GetComponent<Image>();
    }
}