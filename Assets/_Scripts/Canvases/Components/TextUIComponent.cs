using TMPro;
using UnityEngine;

namespace Canvases.Components
{
    public class TextUIComponent : UIComponentBase
    {
        [SerializeField] private TMP_Text text;

        private string initialText;

        private void Awake() => initialText = text.text;

        public void SetText(string newText) => text.text = string.IsNullOrEmpty(newText) ? string.Empty : newText;

        public void ResetText() => text.text = initialText;

        public void AddText(string textToAdd) => text.text += textToAdd;

        public void HideText() => text.text = string.Empty;
    }
}