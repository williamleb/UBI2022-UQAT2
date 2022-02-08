#if UNITY_EDITOR
using System;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;

////TODO: support multi-object editing

namespace InputSystem
{
    /// <summary>
    /// A custom inspector for <see cref="RebindActionUI"/> which provides a more convenient way for
    /// picking the binding which to rebind.
    /// </summary>
    [CustomEditor(typeof(RebindActionUI))]
    public class RebindActionUIEditor : Editor
    {
        protected void OnEnable()
        {
            actionProperty = serializedObject.FindProperty("action");
            bindingIdProperty = serializedObject.FindProperty("bindingId");
            actionLabelProperty = serializedObject.FindProperty("actionLabel");
            bindingTextProperty = serializedObject.FindProperty("bindingText");
            rebindOverlayProperty = serializedObject.FindProperty("rebindOverlay");
            rebindTextProperty = serializedObject.FindProperty("rebindText");
            updateBindingUIEventProperty = serializedObject.FindProperty("updateBindingUIEvent");
            rebindStartEventProperty = serializedObject.FindProperty("rebindStartEvent");
            rebindStopEventProperty = serializedObject.FindProperty("rebindStopEvent");
            displayStringOptionsProperty = serializedObject.FindProperty("displayStringOptions");

            RefreshBindingOptions();
        }

        public override void OnInspectorGUI()
        {
            EditorGUI.BeginChangeCheck();

            // Binding section.
            EditorGUILayout.LabelField(bindingLabel, Styles.BoldLabel);
            using (new EditorGUI.IndentLevelScope())
            {
                EditorGUILayout.PropertyField(actionProperty);

                var newSelectedBinding = EditorGUILayout.Popup(bindingLabel, selectedBindingOption, bindingOptions);
                if (newSelectedBinding != selectedBindingOption)
                {
                    var bindingId = bindingOptionValues[newSelectedBinding];
                    bindingIdProperty.stringValue = bindingId;
                    selectedBindingOption = newSelectedBinding;
                }

                var optionsOld = (InputBinding.DisplayStringOptions)displayStringOptionsProperty.intValue;
                var optionsNew = (InputBinding.DisplayStringOptions)EditorGUILayout.EnumFlagsField(displayOptionsLabel, optionsOld);
                if (optionsOld != optionsNew)
                    displayStringOptionsProperty.intValue = (int)optionsNew;
            }

            // UI section.
            EditorGUILayout.Space();
            EditorGUILayout.LabelField(uiLabel, Styles.BoldLabel);
            using (new EditorGUI.IndentLevelScope())
            {
                EditorGUILayout.PropertyField(actionLabelProperty);
                EditorGUILayout.PropertyField(bindingTextProperty);
                EditorGUILayout.PropertyField(rebindOverlayProperty);
                EditorGUILayout.PropertyField(rebindTextProperty);
            }

            // Events section.
            EditorGUILayout.Space();
            EditorGUILayout.LabelField(eventsLabel, Styles.BoldLabel);
            using (new EditorGUI.IndentLevelScope())
            {
                EditorGUILayout.PropertyField(rebindStartEventProperty);
                EditorGUILayout.PropertyField(rebindStopEventProperty);
                EditorGUILayout.PropertyField(updateBindingUIEventProperty);
            }

            if (EditorGUI.EndChangeCheck())
            {
                serializedObject.ApplyModifiedProperties();
                RefreshBindingOptions();
            }
        }

        protected void RefreshBindingOptions()
        {
            var actionReference = (InputActionReference)actionProperty.objectReferenceValue;
            var action = actionReference?.action;

            if (action == null)
            {
                bindingOptions = Array.Empty<GUIContent>();
                bindingOptionValues = Array.Empty<string>();
                selectedBindingOption = -1;
                return;
            }

            var bindings = action.bindings;
            var bindingCount = bindings.Count;

            bindingOptions = new GUIContent[bindingCount];
            bindingOptionValues = new string[bindingCount];
            selectedBindingOption = -1;

            var currentBindingId = bindingIdProperty.stringValue;
            for (var i = 0; i < bindingCount; ++i)
            {
                var binding = bindings[i];
                var bindingId = binding.id.ToString();
                var haveBindingGroups = !string.IsNullOrEmpty(binding.groups);

                // If we don't have a binding groups (control schemes), show the device that if there are, for example,
                // there are two bindings with the display string "A", the user can see that one is for the keyboard
                // and the other for the gamepad.
                var displayOptions =
                    InputBinding.DisplayStringOptions.DontUseShortDisplayNames | InputBinding.DisplayStringOptions.IgnoreBindingOverrides;
                if (!haveBindingGroups)
                    displayOptions |= InputBinding.DisplayStringOptions.DontOmitDevice;

                // Create display string.
                var displayString = action.GetBindingDisplayString(i, displayOptions);

                // If binding is part of a composite, include the part name.
                if (binding.isPartOfComposite)
                    displayString = $"{ObjectNames.NicifyVariableName(binding.name)}: {displayString}";

                // Some composites use '/' as a separator. When used in popup, this will lead to to submenus. Prevent
                // by instead using a backlash.
                displayString = displayString.Replace('/', '\\');

                // If the binding is part of control schemes, mention them.
                if (haveBindingGroups)
                {
                    var asset = action.actionMap?.asset;
                    if (asset != null)
                    {
                        var controlSchemes = string.Join(", ",
                            binding.groups.Split(InputBinding.Separator)
                                .Select(x => asset.controlSchemes.FirstOrDefault(c => c.bindingGroup == x).name));

                        displayString = $"{displayString} ({controlSchemes})";
                    }
                }

                bindingOptions[i] = new GUIContent(displayString);
                bindingOptionValues[i] = bindingId;

                if (currentBindingId == bindingId)
                    selectedBindingOption = i;
            }
        }

        private SerializedProperty actionProperty;
        private SerializedProperty bindingIdProperty;
        private SerializedProperty actionLabelProperty;
        private SerializedProperty bindingTextProperty;
        private SerializedProperty rebindOverlayProperty;
        private SerializedProperty rebindTextProperty;
        private SerializedProperty rebindStartEventProperty;
        private SerializedProperty rebindStopEventProperty;
        private SerializedProperty updateBindingUIEventProperty;
        private SerializedProperty displayStringOptionsProperty;

        private readonly GUIContent bindingLabel = new GUIContent("Binding");
        private readonly GUIContent displayOptionsLabel = new GUIContent("Display Options");
        private readonly GUIContent uiLabel = new GUIContent("UI");
        private readonly GUIContent eventsLabel = new GUIContent("Events");
        private GUIContent[] bindingOptions;
        private string[] bindingOptionValues;
        private int selectedBindingOption;

        private static class Styles
        {
            public static readonly GUIStyle BoldLabel = new GUIStyle("MiniBoldLabel");
        }
    }
}
#endif
