using System;
using Canvases.Components;
using Managers.Game;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Canvases.HUDs
{
    public class HUDPhaseProgress : MonoBehaviour
    {
        [SerializeField, Required] private SliderUIComponent progressBar;

        private void Start()
        {
            progressBar.Value = 0f;
            
            if (GameManager.HasInstance)
                GameManager.Instance.OnPhaseTotalHomeworkChanged += OnPhaseTotalHomeworkChanged;
        }

        private void OnDestroy()
        {
            if (GameManager.HasInstance)
                GameManager.Instance.OnPhaseTotalHomeworkChanged -= OnPhaseTotalHomeworkChanged;
        }

        private void OnPhaseTotalHomeworkChanged()
        {
            var progress = GameManager.Instance.HomeworksHanded /
                           (float) GameManager.Instance.HomeworksNeededToFinishGame;
            
            progressBar.Value = progress;
        }
    }
}