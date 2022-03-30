using System;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Utilities.Extensions;
using Event = AK.Wwise.Event;

namespace Canvases.Components
{
    // ReSharper disable once InconsistentNaming Reason: UI should be capitalized
    public class ButtonUIComponent : UIComponentBase
    {
        public event Action OnClick;
        public event Action OnSelected;
        
        [Header("Association")] [Required]
        [SerializeField] private Button button;

        [Header("Sound")] 
        [SerializeField] private Event onClickSound;
        [SerializeField] private Event onSelectSound;

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
            GetParentCanvasGroup();
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

        private void GetParentCanvasGroup()
        {
            parentCanvasGroup = null;

            var currentParent = button.gameObject;
            while (currentParent.GetComponent<Canvas>() == null && parentCanvasGroup == null)
            {
                currentParent = currentParent.GetParent();
                parentCanvasGroup = currentParent.GetComponent<CanvasGroup>();
            }
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
            onClickSound?.Post(gameObject);
            OnClick?.Invoke();
        }

        private void OnButtonSelected(BaseEventData data)
        {
            if (!button.interactable || parentCanvasGroup != null && !parentCanvasGroup.interactable)
                return;
            
            onSelectSound?.Post(gameObject);
            OnSelected?.Invoke();
        }

        private void OnValidate()
        {
            if (!button)
                button = GetComponent<Button>();
        }
    }
}