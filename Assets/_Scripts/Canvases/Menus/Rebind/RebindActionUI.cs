using System;
using Canvases.Components;
using UnityEngine;
using UnityEngine.InputSystem;
using Utilities;
using Utilities.Extensions;

namespace Canvases.Menu.Rebind
{
    public class RebindActionUI : MonoBehaviour
    {
        public event Action<string, string> UpdateBindingUIEvent;

        [SerializeField] private TextUIComponent actionLabel;
        [SerializeField] private RebindReferences mainBindingReferences;
        [SerializeField] private RebindReferences altBindingReferences;

        private TextUIComponent rebindOverlay;
        private InputAction action;
        private InputActionRebindingExtensions.RebindingOperation rebindOperation;

        public void ResetToDefault()
        {
            action.actionMap.Disable();
            action.RemoveBindingOverride(mainBindingReferences.Index);
            action.RemoveBindingOverride(altBindingReferences.Index);
            UpdateBindingDisplay();
            action.actionMap.Enable();
        }

        private void StartInteractiveRebind(int index)
        {
            action.actionMap.Disable();
            PerformInteractiveRebind(index);
            action.actionMap.Enable();
        }

        private void PerformInteractiveRebind(int bindingIndex)
        {
            rebindOperation?.Cancel(); // Will null out m_RebindOperation.

            void CleanUp(InputActionRebindingExtensions.RebindingOperation rebindingOperation)
            {
                if (rebindOverlay != null)
                {
                    rebindOverlay.SetActive(false);
                }

                UpdateBindingDisplay();
                rebindOperation?.Dispose();
                rebindOperation = null;
            }

            // Configure the rebind.
            rebindOperation = action.PerformInteractiveRebinding(bindingIndex)
                .OnCancel(CleanUp)
                .OnComplete(CleanUp);

            // If it's a part binding, show the name of the part in the UI.
            string partName = string.Empty;
            if (action.bindings[bindingIndex].isPartOfComposite)
                partName = $"Binding '{action.bindings[bindingIndex].name}'. ";

            // Bring up rebind overlay, if we have one.
            if (rebindOverlay != null)
            {
                rebindOverlay.Show();
                string text = !string.IsNullOrEmpty(rebindOperation.expectedControlType)
                    ? $"{partName}Waiting for {rebindOperation.expectedControlType} input..."
                    : $"{partName}Waiting for input...";
                rebindOverlay.Text =text;
            }

            // If we have no rebind overlay and no callback but we have a binding text label,
            // temporarily set the binding text label to "<Waiting>".
            if (rebindOverlay == null && mainBindingReferences.Text != null)
                mainBindingReferences.Text.Text = "<Waiting...>";

            rebindOperation.Start();
        }

        protected void OnEnable()
        {
            mainBindingReferences.Button.OnClick += () => StartInteractiveRebind(mainBindingReferences.Index);
            altBindingReferences.Button.OnClick += () => StartInteractiveRebind(altBindingReferences.Index);
        }

        protected void OnDisable()
        {
            rebindOperation?.Dispose();
            rebindOperation = null;
        }

        private void UpdateActionLabel()
        {
            if (actionLabel == null) return;

            actionLabel.Text = action.bindings[mainBindingReferences.Index].isPartOfComposite
                ? action.bindings[mainBindingReferences.Index].name.Capitalize()
                : action.name;
        }

        public void UpdateBindingDisplay(bool shouldCallEvent = true)
        {
            string mainDisplayString = string.Empty;
            string altDisplayString = string.Empty;
            string deviceLayoutName = string.Empty;
            string mainControlPath = string.Empty;
            string altControlPath = string.Empty;

            if (action != null)
            {
                if (mainBindingReferences.Index != -1)
                    mainDisplayString = action.GetBindingDisplayString(mainBindingReferences.Index,
                        out deviceLayoutName,
                        out mainControlPath);
                if (altBindingReferences.Index != -1)
                    altDisplayString = action.GetBindingDisplayString(altBindingReferences.Index, out _,
                        out altControlPath);
            }

            UpdateDuplicateText(mainBindingReferences, mainDisplayString);
            UpdateDuplicateText(altBindingReferences, altDisplayString);

            Sprite mainIcon = BindingsIconsUtil.GetSprite(deviceLayoutName, mainControlPath);
            Sprite altIcon = BindingsIconsUtil.GetSprite(deviceLayoutName, altControlPath);

            DisplayIcon(mainBindingReferences, mainIcon);
            DisplayIcon(altBindingReferences, altIcon);

            if (shouldCallEvent)
            {
                UpdateBindingUIEvent?.Invoke(deviceLayoutName, mainControlPath);
            }
        }

        private void DisplayIcon(RebindReferences reference, Sprite mainIcon)
        {
            if (mainIcon != null)
            {
                reference.Text.Hide();
                reference.Image.Sprite = mainIcon;
                reference.Image.Show();
            }
            else
            {
                reference.Text.Show();
                reference.Image.Hide();
            }
        }

        private void UpdateDuplicateText(RebindReferences reference, string mainDisplayString)
        {
            if (reference.Text == null) return;
            reference.Text.Text = mainDisplayString == "Delta" ? "Mouse" : mainDisplayString;
            bool mainDuplicate = CheckDuplicateBindings(reference.Index);
            reference.Button.Color = mainDuplicate ? Color.red : Color.white;
        }

        private bool CheckDuplicateBindings(int bindingIndex)
        {
            if (action == null) return false;

            InputBinding newBinding = action.bindings[bindingIndex];

            if (newBinding.effectivePath.IsNullOrEmpty()) return false;

            foreach (InputBinding binding in action.actionMap.bindings)
            {
                if (binding.action == newBinding.action) continue;

                if (binding.effectivePath == newBinding.effectivePath) return true;
            }

            if (!action.bindings[0].isComposite) return false;

            for (int i = 1; i < action.bindings.Count; i++)
            {
                if (i == bindingIndex) continue;
                if (action.bindings[i].effectivePath == newBinding.effectivePath)
                {
                    return true;
                }
            }

            return false;
        }

        public void Initialize(int bindingIndex, InputAction inputAction, TextUIComponent overlay,
            Action<string, string> onUpdateBindingUIEvent)
        {
            mainBindingReferences.Index = bindingIndex;
            altBindingReferences.Index = bindingIndex + 1;
            action = inputAction;
            rebindOverlay = overlay;
            UpdateBindingUIEvent += onUpdateBindingUIEvent;
            UpdateActionLabel();
            UpdateBindingDisplay();
        }
    }
}