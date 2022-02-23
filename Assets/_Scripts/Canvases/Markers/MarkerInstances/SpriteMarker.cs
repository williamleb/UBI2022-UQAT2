using UnityEngine;
using UnityEngine.UI;

namespace Canvases.Markers
{
    [RequireComponent(typeof(Image))]
    public class SpriteMarker : Marker
    {
        private Image image;

        public Sprite Sprite
        {
            set => image.sprite = value;
        }
        
        protected override void Awake()
        {
            base.Awake();

            image = GetComponent<Image>();
        }
    }
}