using System;
using System.Collections.Generic;
using System.Linq;
using Canvases.Components;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Canvases.Matchmaking
{
    public class HostJoinSequence : MonoBehaviour
    {
        public event Action OnChanged;

        [ValidateInput(nameof(ValidateElements), "There must be at least one element in the sequence")]
        [SerializeField, Required] private List<HostJoinSequenceElement> elements;
        [ValidateInput(nameof(ValidateButtons), "The number of buttons must match the number of available icons in the elements")]
        [SerializeField, Required] private List<ButtonUIComponent> buttons;
        [SerializeField, Required] private ButtonUIComponent backButton;
        
        private int currentIndex = 0;

        public bool IsComplete => currentIndex == elements.Count;
        public string Value => string.Join("-", elements.Select(elem => elem.CurrentValue.ToString()));

        private void Start()
        {
            for (var i = 0; i < buttons.Count; ++i)
            {
                var index = i;
                buttons[i].OnClick += () => OnButtonElementPressed(index);
            }

            if (backButton)
                backButton.OnClick += OnBackButtonPressed;
        }

        private void OnButtonElementPressed(int value)
        {
            if (IsComplete)
                return;
            
            elements[currentIndex].CurrentValue = value;
            currentIndex = Math.Min(currentIndex + 1, elements.Count);
            
            OnChanged?.Invoke();
        }

        private void OnBackButtonPressed()
        {
            currentIndex = Math.Max(0, currentIndex - 1);
            elements[currentIndex].Empty();
            
            OnChanged?.Invoke();
        }

        private bool ValidateElements()
        {
            return elements.Any();
        }

        private bool ValidateButtons()
        {
            if (!ValidateElements())
                return false;
            
            return buttons.Count == elements.First().NumberOfIcons;
        }
    }
}