using Managers.Game;
using System.Threading.Tasks;
using Systems.Settings;
using Systems.Teams;
using TMPro;
using UnityEngine;

public class HUDOvertime : MonoBehaviour
{
    private Color leftTeamColor;
    private Color rightTeamColor;
    private Animator animator;
    private TextMeshProUGUI overtimeText;

    void Start()
    {
        overtimeText = GetComponentInChildren<TextMeshProUGUI>();
        animator = GetComponentInChildren<Animator>();

        if (GameManager.HasInstance)
        {
            GameManager.Instance.OnGameStateChanged += OnGameStateChanged;
        }
    }

    private async void OnGameStateChanged(GameState gameState)
    {
        if (overtimeText == null || animator == null)
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
            animator.SetTrigger("Spawn");
        }
    }

    void Update()
    {
        if (overtimeText == null || leftTeamColor == null || rightTeamColor == null)
            return;

        overtimeText.color = Color.Lerp(rightTeamColor, leftTeamColor, Mathf.PingPong(Time.time, 1));
    }
}
