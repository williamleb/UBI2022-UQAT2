using Managers.Game;
using Sirenix.OdinInspector;
using Systems.Settings;
using Systems.Teams;
using TMPro;
using UnityEngine;

public class HUDOvertime : MonoBehaviour
{
    private Color leftTeamColor;
    private Color rightTeamColor;

    [SerializeField, Required] private TextMeshProUGUI overtimeText;

    void Start()
    {
        if (GameManager.HasInstance)
        {
            GameManager.Instance.OnGameStateChanged += OnGameStateChanged;
        }
    }

    private void OnGameStateChanged(GameState gameState)
    {
        if (gameState == GameState.Running)
        {
            Color.RGBToHSV(SettingsSystem.CustomizationSettings.GetColor(TeamSystem.Instance.Teams[0].Color), out float lH, out float lS, out float _);
            Color.RGBToHSV(SettingsSystem.CustomizationSettings.GetColor(TeamSystem.Instance.Teams[1].Color), out float rH, out float rS, out float _);
            leftTeamColor = Color.HSVToRGB(lH,lS,1);
            rightTeamColor = Color.HSVToRGB(rH, rS, 1);
        }
    }

    void Update()
    {
        if (leftTeamColor == null || rightTeamColor == null)
            return;

        overtimeText.color = Color.Lerp(rightTeamColor, leftTeamColor, Mathf.PingPong(Time.time, 1));
    }
}
