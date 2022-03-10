using Sirenix.OdinInspector;
using Systems.Settings;
using UnityEngine;

namespace Units.AI.Senses
{
    public class TeacherVision : Vision
    {
        [Header("Editor mode only")]
        [SerializeField] private AISettings editorOnlyTeacherSettings;

        private void Awake()
        {
            var settings = SettingsSystem.AISettings;
            Near = settings.VisionNear;
            Far = settings.VisionFar;
            NearLength = settings.VisionNearLength;
            FarLength = settings.VisionFarLength;
        }
        
#if UNITY_EDITOR
        protected override void OnValidate()
        {
            EditorOnlyUpdateFrustumValues();
            
            base.OnValidate();
        }

        [Button("Update values from settings")]
        private void EditorOnlyUpdateFrustumValues()
        {
            if (!editorOnlyTeacherSettings)
                return;

            Near = editorOnlyTeacherSettings.VisionNear;
            Far = editorOnlyTeacherSettings.VisionFar;
            NearLength = editorOnlyTeacherSettings.VisionNearLength;
            FarLength = editorOnlyTeacherSettings.VisionFarLength;
        }
#endif
    }
}