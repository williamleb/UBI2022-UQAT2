using System;
using Canvases.Components;
using Canvases.Animations;
using Sirenix.OdinInspector;
using Units.Player;
using UnityEngine;

namespace Canvases.Menu
{
    [RequireComponent(typeof(CanvasGroup))]
    public abstract class AbstractMenu : MonoBehaviour, IMenu
    {
        protected enum EntryDirection { Down, Up }
        
        public event Action OnShow; 
        public event Action OnHide;
        
        [SerializeField, Required] private ButtonUIComponent firstButtonToFocus;

        [SerializeField, Required] private EntryAnimation entry;
        private CanvasGroup canvasGroup;

        protected abstract EntryDirection EnterDirection { get; }
        protected abstract EntryDirection LeaveDirection { get; }

        public virtual void ShowFor(PlayerEntity playerEntity)
        {
            if (entry.IsEnteredOrEntering)
                return;

            ShowForImplementation(playerEntity);
            
            entry.EnterDown();
            OnShow?.Invoke();
        }

        public virtual void Show()
        {
            if (entry.IsEnteredOrEntering)
                return;

            ShowImplementation();
            
            entry.EnterDown();
            OnShow?.Invoke();
        }

        public virtual void Hide()
        {
            if (entry.IsLeftOrLeaving)
                return;
            
            HideImplementation();

            entry.LeaveDown();
            canvasGroup.interactable = false;
        }

        public virtual void ShowForImplementation(PlayerEntity playerEntity) { ShowImplementation(); }
        public virtual void ShowImplementation() {  }
        public virtual void HideImplementation() {  }

        protected virtual void Awake()
        {
            canvasGroup = GetComponent<CanvasGroup>();
        }
        
        protected virtual void OnEnable()
        {
            entry.OnEntered += OnEntered;
            entry.OnLeft += OnLeft;
        }

        protected virtual void OnDisable()
        {
            entry.OnEntered -= OnEntered;
            entry.OnLeft -= OnLeft;
        }
        
        private void OnEntered()
        {
            canvasGroup.interactable = true;
            firstButtonToFocus.Select();
        }
        
        private void OnLeft()
        {
            OnHide?.Invoke();
        }
    }
}