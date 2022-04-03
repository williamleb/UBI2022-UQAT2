using System;
using Sirenix.OdinInspector;
using Systems.Sound;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Utilities.Extensions;

namespace Canvases.Components
{
    public class ButtonUIComponent : UIComponentBase
    {
        private enum Type {Forward, Backward}
        
        public event Action OnClick;
        public event Action OnSelected;
        
        [SerializeField, Required] private Button button;
        [SerializeField] private Type type;

        private EventTrigger.Entry selectEventTriggerEntry;
        private CanvasGroup parentCanvasGroup;
        
        public Color Color
        {
            set => button.image.color = value;
        }
        
        public bool Enabled
        {
            set => button.interactable = value;
        }

        public void Select() => button.Select();

        private void Awake()
        {
            AddSelectEventTrigger();
            parentCanvasGroup = gameObject.GetComponentInParents<CanvasGroup>();
        }

        private void AddSelectEventTrigger()
        {
            var eventTrigger = button.gameObject.GetOrAddComponent<EventTrigger>();
            selectEventTriggerEntry = new EventTrigger.Entry
            {
                eventID = EventTriggerType.Select
            };
            selectEventTriggerEntry.callback.AddListener(OnButtonSelected);
            eventTrigger.triggers.Add(selectEventTriggerEntry);
        }

        private void Start()
        {
            Debug.Assert(button, $"A {nameof(button)} must be assigned to a {nameof(ButtonUIComponent)}");
            button.onClick.AddListener(OnButtonClicked);
        }

        private void OnDestroy()
        {
            button.onClick.RemoveListener(OnButtonClicked);
            
            if (selectEventTriggerEntry != null)
                selectEventTriggerEntry.callback.RemoveListener(OnButtonSelected);
        }
        
        private void OnButtonClicked()
        {
            PlayClickedSound();
            OnClick?.Invoke();
        }

        private void PlayClickedSound()
        {
            switch (type)
            {
                case Type.Forward:
                    SoundSystem.Instance.PlayMenuElementForwardSound();
                    break;
                case Type.Backward:
                    SoundSystem.Instance.PlayMenuElementBackwardSound();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void OnButtonSelected(BaseEventData data)
        {
            if (!button.interactable || parentCanvasGroup != null && !parentCanvasGroup.interactable)
                return;
            
            
            SoundSystem.Instance.PlayMenuElementSelectSound();
            OnSelected?.Invoke();
        }

        private void OnValidate()
        {
            if (!button)
                button = GetComponent<Button>();
        }
    }
}