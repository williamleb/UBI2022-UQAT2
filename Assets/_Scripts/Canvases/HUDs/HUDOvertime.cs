using System;
using System.Threading.Tasks;
using Canvases.Components;
using Managers.Game;
using Systems.Settings;
using Systems.Teams;
using UnityEngine;

namespace Canvases.HUDs
{
    public class HUDOvertime : MonoBehaviour
    {
        [SerializeField] private TextUIComponent overtimeText;

        private Color leftTeamColor;
        private Color rightTeamColor;

        private void Awake()
        {
            if (overtimeText == null)
            {
                Debug.LogWarning("Missing TextUIComponent on Overtime text.");
                return;
            }
            overtimeText.Hide();
        }

        private void Start()
        {
            if (GameManager.HasInstance)
                GameManager.Instance.OnGameStateChanged += OnGameStateChanged;
        }

        private void OnDestroy()
        {
            if (GameManager.HasInstance)
                GameManager.Instance.OnGameStateChanged -= OnGameStateChanged;
        }

        private async void OnGameStateChanged(GameState gameState)
        {
            if (overtimeText == null)
            {
                Debug.LogWarning("Missing component on Overtime text. Not displaying it.");
                return;
            }

            if (gameState == GameState.Running)
            {
                Color.RGBToHSV(SettingsSystem.CustomizationSettings.GetColor(TeamSystem.Instance.Teams[0].Color), out float lH, out float lS, out float _);
                Color.RGBToHSV(SettingsSystem.CustomizationSettings.GetColor(TeamSystem.Instance.Teams[1].Color), out float rH, out float rS, out float _);
                leftTeamColor = Color.HSVToRGB(lH,lS,1);
                rightTeamColor = Color.HSVToRGB(rH, rS, 1);
            }

            if(gameState == GameState.Overtime)
            {
                await Task.Delay(50);
                overtimeText.Show();
            }
        }

        private void Update()
        {
            if (overtimeText == null)
                return;

            overtimeText.Color = Color.Lerp(rightTeamColor, leftTeamColor, Mathf.PingPong(Time.time, 1));
        }

        private void OnValidate()
        {
            overtimeText = GetComponentInChildren<TextUIComponent>();
        }
    }
}
