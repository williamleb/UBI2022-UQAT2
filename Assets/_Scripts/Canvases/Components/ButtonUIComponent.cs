using System;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;
using Event = AK.Wwise.Event;

namespace Canvases.Components
{
    // ReSharper disable once InconsistentNaming Reason: UI should be capitalized
    public class ButtonUIComponent : UIComponentBase
    {
        public event Action OnClick;
        
        [Header("Association")] [Required]
        [SerializeField] private Button button;

        [Header("Sound")] 
        [SerializeField] private Event onClickSound;

        private void Start()
        {
            Debug.Assert(button, $"A {nameof(button)} must be assigned to a {nameof(ButtonUIComponent)}");
            button.onClick.AddListener(OnButtonClicked);
        }

        private void OnDestroy()
        {
            button.onClick.RemoveListener(OnButtonClicked);
        }
        
        private void OnButtonClicked()
        {
            onClickSound?.Post(gameObject);
            OnClick?.Invoke();
        }

        private void OnValidate()
        {
            if (!button)
                button = GetComponent<Button>();
        }
    }
}