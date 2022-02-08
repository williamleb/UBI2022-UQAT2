using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

////TODO: localization support

////TODO: deal with composites that have parts bound in different control schemes

namespace InputSystem
{
    /// <summary>
    /// A reusable component with a self-contained UI for rebinding a single action.
    /// </summary>
    public class RebindActionUI : MonoBehaviour
    {
        /// <summary>
        /// Reference to the action that is to be rebound.
        /// </summary>
        public InputActionReference ActionReference
        {
            get => action;
            set
            {
                action = value;
                UpdateActionLabel();
                UpdateBindingDisplay();
            }
        }

        /// <summary>
        /// ID (in string form) of the binding that is to be rebound on the action.
        /// </summary>
        /// <seealso cref="InputBinding.id"/>
        public string BindingId
        {
            get => bindingId;
            set
            {
                bindingId = value;
                UpdateBindingDisplay();
            }
        }

        public InputBinding.DisplayStringOptions DisplayStringOptions
        {
            get => displayStringOptions;
            set
            {
                displayStringOptions = value;
                UpdateBindingDisplay();
            }
        }

        /// <summary>
        /// Text component that receives the name of the action. Optional.
        /// </summary>
        public TMP_Text ActionLabel
        {
            get => actionLabel;
            set
            {
                actionLabel = value;
                UpdateActionLabel();
            }
        }

        /// <summary>
        /// Text component that receives the display string of the binding. Can be <c>null</c> in which
        /// case the component entirely relies on <see cref="UpdateBindingUIEvent"/>.
        /// </summary>
        public TMP_Text BindingText
        {
            get => bindingText;
            set
            {
                bindingText = value;
                UpdateBindingDisplay();
            }
        }

        /// <summary>
        /// Optional text component that receives a text prompt when waiting for a control to be actuated.
        /// </summary>
        /// <seealso cref="StartRebindEvent"/>
        /// <seealso cref="RebindOverlay"/>
        public Text RebindPrompt
        {
            get => rebindText;
            set => rebindText = value;
        }

        /// <summary>
        /// Optional UI that is activated when an interactive rebind is started and deactivated when the rebind
        /// is finished. This is normally used to display an overlay over the current UI while the system is
        /// waiting for a control to be actuated.
        /// </summary>
        /// <remarks>
        /// If neither <see cref="RebindPrompt"/> nor <c>rebindOverlay</c> is set, the component will temporarily
        /// replaced the <see cref="BindingText"/> (if not <c>null</c>) with <c>"Waiting..."</c>.
        /// </remarks>
        /// <seealso cref="StartRebindEvent"/>
        /// <seealso cref="RebindPrompt"/>
        public GameObject RebindOverlay
        {
            get => rebindOverlay;
            set => rebindOverlay = value;
        }

        /// <summary>
        /// Event that is triggered every time the UI updates to reflect the current binding.
        /// This can be used to tie custom visualizations to bindings.
        /// </summary>
        public UpdateBindingUIEvent UpdateBindingUIEvent
        {
            get
            {
                if (updateBindingUIEvent == null)
                    updateBindingUIEvent = new UpdateBindingUIEvent();
                return updateBindingUIEvent;
            }
        }

        /// <summary>
        /// Event that is triggered when an interactive rebind is started on the action.
        /// </summary>
        public InteractiveRebindEvent StartRebindEvent
        {
            get
            {
                if (rebindStartEvent == null)
                    rebindStartEvent = new InteractiveRebindEvent();
                return rebindStartEvent;
            }
        }

        /// <summary>
        /// Event that is triggered when an interactive rebind has been completed or canceled.
        /// </summary>
        public InteractiveRebindEvent StopRebindEvent
        {
            get
            {
                if (rebindStopEvent == null)
                    rebindStopEvent = new InteractiveRebindEvent();
                return rebindStopEvent;
            }
        }

        /// <summary>
        /// When an interactive rebind is in progress, this is the rebind operation controller.
        /// Otherwise, it is <c>null</c>.
        /// </summary>
        public InputActionRebindingExtensions.RebindingOperation OngoingRebind => rebindOperation;

        /// <summary>
        /// Return the action and binding index for the binding that is targeted by the component
        /// according to
        /// </summary>
        /// <param name="action"></param>
        /// <param name="bindingIndex"></param>
        /// <returns></returns>
        public bool ResolveActionAndBinding(out InputAction action, out int bindingIndex)
        {
            bindingIndex = -1;

            action = this.action?.action;
            if (action == null)
                return false;

            if (string.IsNullOrEmpty(this.bindingId))
                return false;

            // Look up binding index.
            var bindingId = new Guid(this.bindingId);
            bindingIndex = action.bindings.IndexOf(x => x.id == bindingId);
            if (bindingIndex == -1)
            {
                Debug.LogError($"Cannot find binding with ID '{bindingId}' on '{action}'", this);
                return false;
            }

            return true;
        }

        /// <summary>
        /// Trigger a refresh of the currently displayed binding.
        /// </summary>
        public void UpdateBindingDisplay()
        {
            var displayString = string.Empty;
            var deviceLayoutName = default(string);
            var controlPath = default(string);

            // Get display string from action.
            var action = this.action?.action;
            if (action != null)
            {
                var bindingIndex = action.bindings.IndexOf(x => x.id.ToString() == bindingId);
                if (bindingIndex != -1)
                    displayString = action.GetBindingDisplayString(bindingIndex, out deviceLayoutName, out controlPath, DisplayStringOptions);
            }

            // Set on label (if any).
            if (bindingText != null)
                bindingText.text = displayString;

            // Give listeners a chance to configure UI in response.
            updateBindingUIEvent?.Invoke(this, displayString, deviceLayoutName, controlPath);
        }

        /// <summary>
        /// Remove currently applied binding overrides.
        /// </summary>
        public void ResetToDefault()
        {
            if (!ResolveActionAndBinding(out var action, out var bindingIndex))
                return;

            if (action.bindings[bindingIndex].isComposite)
            {
                // It's a composite. Remove overrides from part bindings.
                for (var i = bindingIndex + 1; i < action.bindings.Count && action.bindings[i].isPartOfComposite; ++i)
                    action.RemoveBindingOverride(i);
            }
            else
            {
                action.RemoveBindingOverride(bindingIndex);
            }
            UpdateBindingDisplay();
        }

        /// <summary>
        /// Initiate an interactive rebind that lets the player actuate a control to choose a new binding
        /// for the action.
        /// </summary>
        public void StartInteractiveRebind()
        {
            if (!ResolveActionAndBinding(out var action, out var bindingIndex))
                return;

            // If the binding is a composite, we need to rebind each part in turn.
            if (action.bindings[bindingIndex].isComposite)
            {
                var firstPartIndex = bindingIndex + 1;
                if (firstPartIndex < action.bindings.Count && action.bindings[firstPartIndex].isPartOfComposite)
                    PerformInteractiveRebind(action, firstPartIndex, allCompositeParts: true);
            }
            else
            {
                PerformInteractiveRebind(action, bindingIndex);
            }
        }

        private void PerformInteractiveRebind(InputAction action, int bindingIndex, bool allCompositeParts = false)
        {
            rebindOperation?.Cancel(); // Will null out m_RebindOperation.

            void CleanUp()
            {
                rebindOperation?.Dispose();
                rebindOperation = null;
            }

            // Configure the rebind.
            rebindOperation = action.PerformInteractiveRebinding(bindingIndex)
                .OnCancel(
                    operation =>
                    {
                        rebindStopEvent?.Invoke(this, operation);
                        rebindOverlay?.SetActive(false);
                        UpdateBindingDisplay();
                        CleanUp();
                    })
                .OnComplete(
                    operation =>
                    {
                        rebindOverlay?.SetActive(false);
                        rebindStopEvent?.Invoke(this, operation);
                        UpdateBindingDisplay();
                        CleanUp();

                        // If there's more composite parts we should bind, initiate a rebind
                        // for the next part.
                        if (allCompositeParts)
                        {
                            var nextBindingIndex = bindingIndex + 1;
                            if (nextBindingIndex < action.bindings.Count && action.bindings[nextBindingIndex].isPartOfComposite)
                                PerformInteractiveRebind(action, nextBindingIndex, true);
                        }
                    });

            // If it's a part binding, show the name of the part in the UI.
            var partName = default(string);
            if (action.bindings[bindingIndex].isPartOfComposite)
                partName = $"Binding '{action.bindings[bindingIndex].name}'. ";

            // Bring up rebind overlay, if we have one.
            rebindOverlay?.SetActive(true);
            if (rebindText != null)
            {
                var text = !string.IsNullOrEmpty(rebindOperation.expectedControlType)
                    ? $"{partName}Waiting for {rebindOperation.expectedControlType} input..."
                    : $"{partName}Waiting for input...";
                rebindText.text = text;
            }

            // If we have no rebind overlay and no callback but we have a binding text label,
            // temporarily set the binding text label to "<Waiting>".
            if (rebindOverlay == null && rebindText == null && rebindStartEvent == null && bindingText != null)
                bindingText.text = "<Waiting...>";

            // Give listeners a chance to act on the rebind starting.
            rebindStartEvent?.Invoke(this, rebindOperation);

            rebindOperation.Start();
        }

        protected void OnEnable()
        {
            if (rebindActionUIs == null)
                rebindActionUIs = new List<RebindActionUI>();
            rebindActionUIs.Add(this);
            if (rebindActionUIs.Count == 1)
                UnityEngine.InputSystem.InputSystem.onActionChange += OnActionChange;
        }

        protected void OnDisable()
        {
            rebindOperation?.Dispose();
            rebindOperation = null;

            rebindActionUIs.Remove(this);
            if (rebindActionUIs.Count == 0)
            {
                rebindActionUIs = null;
                UnityEngine.InputSystem.InputSystem.onActionChange -= OnActionChange;
            }
        }

        // When the action system re-resolves bindings, we want to update our UI in response. While this will
        // also trigger from changes we made ourselves, it ensures that we react to changes made elsewhere. If
        // the user changes keyboard layout, for example, we will get a BoundControlsChanged notification and
        // will update our UI to reflect the current keyboard layout.
        private static void OnActionChange(object obj, InputActionChange change)
        {
            if (change != InputActionChange.BoundControlsChanged)
                return;

            var action = obj as InputAction;
            var actionMap = action?.actionMap ?? obj as InputActionMap;
            var actionAsset = actionMap?.asset ?? obj as InputActionAsset;

            for (var i = 0; i < rebindActionUIs.Count; ++i)
            {
                var component = rebindActionUIs[i];
                var referencedAction = component.ActionReference?.action;
                if (referencedAction == null)
                    continue;

                if (referencedAction == action ||
                    referencedAction.actionMap == actionMap ||
                    referencedAction.actionMap?.asset == actionAsset)
                    component.UpdateBindingDisplay();
            }
        }

        [Tooltip("Reference to action that is to be rebound from the UI.")]
        [SerializeField]
        private InputActionReference action;

        [SerializeField]
        private string bindingId;

        [SerializeField]
        private InputBinding.DisplayStringOptions displayStringOptions;

        [Tooltip("Text label that will receive the name of the action. Optional. Set to None to have the "
            + "rebind UI not show a label for the action.")]
        [SerializeField]
        private TMP_Text actionLabel;

        [Tooltip("Text label that will receive the current, formatted binding string.")]
        [SerializeField]
        private TMP_Text bindingText;

        [Tooltip("Optional UI that will be shown while a rebind is in progress.")]
        [SerializeField]
        private GameObject rebindOverlay;

        [Tooltip("Optional text label that will be updated with prompt for user input.")]
        [SerializeField]
        private Text rebindText;

        [Tooltip("Event that is triggered when the way the binding is display should be updated. This allows displaying "
            + "bindings in custom ways, e.g. using images instead of text.")]
        [SerializeField]
        private UpdateBindingUIEvent updateBindingUIEvent;

        [Tooltip("Event that is triggered when an interactive rebind is being initiated. This can be used, for example, "
            + "to implement custom UI behavior while a rebind is in progress. It can also be used to further "
            + "customize the rebind.")]
        [SerializeField]
        private InteractiveRebindEvent rebindStartEvent;

        [Tooltip("Event that is triggered when an interactive rebind is complete or has been aborted.")]
        [SerializeField]
        private InteractiveRebindEvent rebindStopEvent;

        private InputActionRebindingExtensions.RebindingOperation rebindOperation;

        private static List<RebindActionUI> rebindActionUIs;

        // We want the label for the action name to update in edit mode, too, so
        // we kick that off from here.
        #if UNITY_EDITOR
        protected void OnValidate()
        {
            UpdateActionLabel();
            UpdateBindingDisplay();
        }

        #endif

        private void UpdateActionLabel()
        {
            if (actionLabel != null)
            {
                var action = this.action?.action;
                actionLabel.text = action != null ? action.name : string.Empty;
            }
        }
        
    }
}
