using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

namespace Canvases.Components
{
    // ReSharper disable once InconsistentNaming Reason: UI should be capitalized
    public class ImageUIComponent : UIComponentBase
    {
        [Header("Association")][Required]
        [SerializeField] private Image image;

        private Color initialColor;

        private void Awake() => initialColor = image.color;
        
        private void Start()
        {
            Debug.Assert(image, $"A {nameof(image)} must be assigned to a {nameof(ImageUIComponent)}");
        }

        public void ResetColor() => image.color = initialColor;
        
        public Sprite Sprite
        {
            set => image.sprite = value;
        }

        public Color Color
        {
            set => image.color = value;
        }

        private void OnValidate()
        {
            if (!image)
                image = GetComponent<Image>();
        }
    }
}