using UnityEngine;

[CreateAssetMenu(menuName = "Settings/Team Settings")]
public class TeamSettings : ScriptableObject
{
    [Tooltip("Maximum number of players per team")] [SerializeField]
    private int maxPlayerPerTeam = 4;

    [Tooltip("Number of team")] [SerializeField]
    private int numberOfTeam = 2;

    [Tooltip("Allow tackling between players of the same team")] [SerializeField] 
    private bool allowFriendlyFire = true;

    [Tooltip("Possible team names")] [SerializeField]
    private string[] teamNames = {
        "Les Poil",
        "Les Yo",
        "Team Mullets",
        "Ti-Bum",
        "Babines",
        "Les grands slack"
    };

    public int MaxPlayerPerTeam => maxPlayerPerTeam;
    public int NumberOfTeam => numberOfTeam;
    public bool AllowFriendlyFire => allowFriendlyFire;
    public string[] TeamNames => teamNames;
}
