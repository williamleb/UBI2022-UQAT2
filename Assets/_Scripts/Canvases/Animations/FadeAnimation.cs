using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using BehaviorDesigner.Runtime.Tasks;
using Sirenix.OdinInspector;
using UnityEngine;
using Action = System.Action;

namespace Canvases.Animations
{
    [RequireComponent(typeof(Animator))]
    [RequiredComponent(typeof(CanvasGroup))]
    public class FadeAnimation : MonoBehaviour
    {
        private static readonly int FadeInTrigger = Animator.StringToHash("FadeIn");
        private static readonly int FadeOutTrigger = Animator.StringToHash("FadeOut");

        private enum Animations {FadeIn, FadeOut}
        
        public event Action OnFadedIn;
        public event Action OnFadedOut;
        
        private Animator animator;
        private CanvasGroup canvasGroup;

        private bool isTransitioning;
        private bool isFadingIn;
        private bool isFadedIn;
        private Queue<Animations> animationsToPlay = new Queue<Animations>();

        public bool IsFadedIn => isFadedIn;
        public bool IsFadedOut => !isFadedIn;

        private void Awake()
        {
            animator = GetComponent<Animator>();
            canvasGroup = GetComponent<CanvasGroup>();
        }

        private void Start()
        {
            isFadingIn = isFadedIn = canvasGroup.alpha > 0;
        }

        public void FadeIn()
        {
            if (!CanPlayFadeIn())
                return;
            
            animationsToPlay.Enqueue(Animations.FadeIn);
            PlayNextAnimationIfOnlyOneInQueue();
        }
        
        public void FadeOut()
        {
            if (!CanPlayFadeOutAnimation())
                return;
            
            animationsToPlay.Enqueue(Animations.FadeOut);
            PlayNextAnimationIfOnlyOneInQueue();
        }
        
        private bool CanPlayFadeIn()
        {
            if (animationsToPlay.Any() && (animationsToPlay.Last() == Animations.FadeIn))
                return false;

            if (!animationsToPlay.Any() && isFadedIn)
                return false;
            
            return true;
        }
        
        private bool CanPlayFadeOutAnimation()
        {
            if (animationsToPlay.Any() && (animationsToPlay.Last() == Animations.FadeOut))
                return false;

            if (!animationsToPlay.Any() && !isFadedIn)
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
            var fadingIn = animationToPlay == Animations.FadeIn;

            StartCoroutine(PlayAnimationRoutine(triggerToPlay, fadingIn));
        }

        private IEnumerator PlayAnimationRoutine(int trigger, bool entering)
        {
            isTransitioning = true;
            isFadingIn = entering;
            
            animator.SetTrigger(trigger);
            yield return null;
            
            yield return new WaitUntil(IsAnimationFinished);

            isFadedIn = entering;
            if (entering)
                OnFadedIn?.Invoke();
            else
                OnFadedOut?.Invoke();

            isTransitioning = false;
            PlayNextAnimation();
        }

        private int TriggerFromAnimation(Animations animationToPlay)
        {
            switch (animationToPlay)
            {
                case Animations.FadeIn:
                    return FadeInTrigger;
                case Animations.FadeOut:
                    return FadeOutTrigger;
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
                    FadeIn();
                }

                if (GUI.Button(new Rect(0, 40, 200, 40), "EnterDown"))
                {
                    FadeOut();
                }
            }
        }
#endif
    }
}