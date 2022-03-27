﻿using System;
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

        public bool ShowFor(PlayerEntity playerEntity)
        {
            if (entry.IsEnteredOrEntering)
                return false;

            if (!ShowForImplementation(playerEntity))
                return false;
            
            Enter();
            OnShow?.Invoke();
            return true;
        }

        public bool Show()
        {
            if (entry.IsEnteredOrEntering)
                return false;

            if (!ShowImplementation())
                return false;
            
            Enter();
            OnShow?.Invoke();
            return true;
        }

        public virtual bool Hide()
        {
            if (entry.IsLeftOrLeaving)
                return false;

            if (!HideImplementation())
                return false;

            Leave();
            canvasGroup.interactable = false;
            return true;
        }

        private void Enter()
        {
            if (EnterDirection == EntryDirection.Down)
                entry.EnterDown();
            else 
                entry.EnterUp();
        }

        private void Leave()
        {
            if (LeaveDirection == EntryDirection.Down)
                entry.LeaveDown();
            else 
                entry.LeaveUp();
        }

        public virtual bool ShowForImplementation(PlayerEntity playerEntity) => ShowImplementation();
        public virtual bool ShowImplementation() => true;
        public virtual bool HideImplementation() => true;

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
            if (MenuManager.HasInstance)
            {
                MenuManager.Instance.PushButtonToReturnTo(firstButtonToFocus);
            }
            
            canvasGroup.interactable = true;
            firstButtonToFocus.Select();
        }
        
        private void OnLeft()
        {
            if (MenuManager.HasInstance)
            {
                MenuManager.Instance.RemoveButtonToReturnTo(firstButtonToFocus);
                MenuManager.Instance.ReturnToButton();
            }
            
            OnHide?.Invoke();
        }
    }
}