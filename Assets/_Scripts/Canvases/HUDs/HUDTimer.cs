using Managers.Game;
using System;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(TextMeshProUGUI))]
public class HUDTimer : MonoBehaviour
{
    private TextMeshProUGUI text;
    private GameTimer gameTimer;

    void Start()
    {
        text = GetComponent<TextMeshProUGUI>();

        if (GameManager.HasInstance)
            gameTimer = GameManager.Instance.GameTimer;

        Debug.Assert(gameTimer);
    }

    void Update()
    {
        if (!GameManager.HasInstance)
            return;
        
        if (GameManager.Instance.CurrentState == GameState.Running || GameManager.Instance.CurrentState == GameState.Overtime)
        {
            var span = TimeSpan.FromSeconds(Convert.ToDouble(gameTimer.RemainingTime));
            text.SetText($"{span.Minutes:00}:{span.Seconds:00}");
        }
    }
}
