using System;
using Canvases.Animations;
using Canvases.Components;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Canvases.TransitionScreen
{
    public class TransitionScreen : MonoBehaviour
    {
        public event Action OnShown
        {
            add => fade.OnFadedIn += value;
            remove => fade.OnFadedIn -= value;
        }
        
        public event Action OnHidden
        {
            add => fade.OnFadedOut += value;
            remove => fade.OnFadedOut -= value;
        }
        
        [SerializeField, Required] private FadeAnimation fade;
        [SerializeField, Required] private TextUIComponent text;

        public bool IsShown => fade.IsFadedIn;
        public bool IsHidden => fade.IsFadedOut;

        public void Show() => fade.FadeIn();
        public void Hide() => fade.FadeOut();

        public void ChangeText(string newText)
        {
            text.Text = $"{newText}...";
        }
    }
}