using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;

namespace Canvases.Components
{
    // ReSharper disable once InconsistentNaming Reason: UI should be capitalized
    public class TextUIComponent : UIComponentBase
    {
        [Header("Association")] [Required]
        [SerializeField] private TMP_Text text;

        private string initialText;

        public string Text
        {
            set => text.text = string.IsNullOrEmpty(value) ? string.Empty : value;
        }
        
        public Color Color
        {
            get => text.color;
            set => text.color = value;
        }

        private void Awake() => initialText = text.text;
        
        private void Start()
        {
            Debug.Assert(text, $"A {nameof(text)} must be assigned to a {nameof(TextUIComponent)}");
        }


        public void ResetText() => text.text = initialText;

        public void AddText(string textToAdd) => text.text += textToAdd;

        public void EraseText() => text.text = string.Empty;

        private void OnValidate()
        {
            if (!text)
                text = GetComponent<TMP_Text>();
        }
    }
}