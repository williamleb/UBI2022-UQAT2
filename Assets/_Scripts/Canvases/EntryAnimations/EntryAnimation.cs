﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Canvases.EntryAnimations
{
    [RequireComponent(typeof(Animator))]
    public class EntryAnimation : MonoBehaviour
    {
        private static readonly int enterUp = Animator.StringToHash("EnterUp");
        private static readonly int enterDown = Animator.StringToHash("EnterDown");
        private static readonly int leaveUp = Animator.StringToHash("LeaveUp");
        private static readonly int leaveDown = Animator.StringToHash("LeaveDown");

        private enum Animations {EnterUp, EnterDown, LeaveUp, LeaveDown}
        
        public event Action OnEntered;
        public event Action OnLeft;
        
        private Animator animator;

        private bool isTransitioning;
        private bool isEntering;
        private bool isEntered;
        private Queue<Animations> animationsToPlay = new Queue<Animations>();

        public bool IsEntered => isEntered;

        private void Awake()
        {
            animator = GetComponent<Animator>();
        }

        public void EnterUp()
        {
            if (!CanPlayEnterAnimation())
                return;
            
            animationsToPlay.Enqueue(Animations.EnterUp);
            PlayNextAnimationIfOnlyOneInQueue();
        }
        
        public void EnterDown()
        {
            if (!CanPlayEnterAnimation())
                return;
            
            animationsToPlay.Enqueue(Animations.EnterDown);
            PlayNextAnimationIfOnlyOneInQueue();
        }

        public void LeaveUp()
        {
            if (!CanPlayLeaveAnimation())
                return;
            
            animationsToPlay.Enqueue(Animations.LeaveUp);
            PlayNextAnimationIfOnlyOneInQueue();
        }
        
        public void LeaveDown()
        {
            if (!CanPlayLeaveAnimation())
                return;
            
            animationsToPlay.Enqueue(Animations.LeaveDown);
            PlayNextAnimationIfOnlyOneInQueue();
        }

        private bool CanPlayEnterAnimation()
        {
            if (animationsToPlay.Any() && (animationsToPlay.Last() == Animations.EnterUp || animationsToPlay.Last() == Animations.EnterDown))
                return false;

            if (!animationsToPlay.Any() && isEntering)
                return false;
            
            return true;
        }
        
        private bool CanPlayLeaveAnimation()
        {
            if (animationsToPlay.Any() && (animationsToPlay.Last() == Animations.LeaveUp || animationsToPlay.Last() == Animations.LeaveDown))
                return false;

            if (!animationsToPlay.Any() && !isEntering)
                return false;
            
            return true;
        }

        private void PlayNextAnimationIfOnlyOneInQueue()
        {
            if (animationsToPlay.Count == 1 && !isTransitioning)
                PlayNextAnimation();
        }

        private void PlayNextAnimation()
        {
            if (!animationsToPlay.Any())
                return;
            
            var animationToPlay = animationsToPlay.Dequeue();
            var triggerToPlay = TriggerFromAnimation(animationToPlay);
            var entering = animationToPlay == Animations.EnterDown || animationToPlay == Animations.EnterUp;

            StartCoroutine(PlayAnimationRoutine(triggerToPlay, entering));
        }

        private IEnumerator PlayAnimationRoutine(int trigger, bool entering)
        {
            isTransitioning = true;
            isEntering = entering;
            
            animator.SetTrigger(trigger);
            yield return null;
            
            yield return new WaitUntil(IsAnimationFinished);

            if (entering) // TODO Remove
                Debug.Log("Entered ===========");
            else 
                Debug.Log("Left ============");

            isEntered = entering;
            if (entering)
                OnEntered?.Invoke();
            else
                OnLeft?.Invoke();

            isTransitioning = false;
            PlayNextAnimation();
        }

        private int TriggerFromAnimation(Animations animationToPlay)
        {
            switch (animationToPlay)
            {
                case Animations.EnterUp:
                    return enterUp;
                case Animations.EnterDown:
                    return enterDown;
                case Animations.LeaveUp:
                    return leaveUp;
                case Animations.LeaveDown:
                    return leaveDown;
                default:
                    throw new ArgumentOutOfRangeException(nameof(animationToPlay), animationToPlay, null);
            }
        }

        private bool IsAnimationFinished()
        {
            return animator.GetCurrentAnimatorStateInfo(0).normalizedTime > 1 && !animator.IsInTransition(0);
        }
        
        #if UNITY_EDITOR
        
        private bool showDebugMenu;
        
        [Button("ToggleDebugMenu")]
        private void ToggleDebugMenu()
        {
            showDebugMenu = !showDebugMenu;
        }

        private void OnGUI()
        {
            if (showDebugMenu)
            {
                if (GUI.Button(new Rect(0, 0, 200, 40), "EnterUp"))
                {
                    EnterUp();
                }

                if (GUI.Button(new Rect(0, 40, 200, 40), "EnterDown"))
                {
                    EnterDown();
                }
                
                if (GUI.Button(new Rect(0, 80, 200, 40), "LeaveUp"))
                {
                    LeaveUp();
                }
                
                if (GUI.Button(new Rect(0, 120, 200, 40), "LeaveDown"))
                {
                    LeaveDown();
                }
            }
        }
#endif
    }
}