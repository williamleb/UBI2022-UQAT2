using System;
using System.Collections.Generic;
using Canvases.EntryAnimations;
using Sirenix.OdinInspector;
using Units.Player;
using UnityEngine;

namespace Canvases.Customization
{
    [RequireComponent(typeof(CanvasGroup))]
    public class CustomizationUI : MonoBehaviour
    {
        [SerializeField, Required] private EntryAnimation entry;

        private CanvasGroup canvasGroup;
        private CustomizationUIElement[] customizationUIElements;

        private void Awake()
        {
            canvasGroup = GetComponent<CanvasGroup>();
            customizationUIElements = GetComponentsInChildren<CustomizationUIElement>();
        }

        private void OnEnable()
        {
            entry.OnEntered += OnEntered;
        }

        private void OnDisable()
        {
            entry.OnEntered -= OnEntered;
        }

        public void ShowFor(PlayerEntity playerEntity)
        {
            if (entry.IsEnteredOrEntering)
                return;
            
            foreach (var element in customizationUIElements)
            {
                element.Activate(playerEntity);
            }
            
            entry.EnterUp();
        }
        
        private void OnEntered()
        {
            canvasGroup.interactable = true;
            // TODO Select first button
        }

        public void Hide()
        {
            if (entry.IsLeftOrLeaving)
                return;

            canvasGroup.interactable = false;
        }
    }
}