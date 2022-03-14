using Sirenix.OdinInspector;
using UnityEngine;

namespace Systems.Settings
{
    [CreateAssetMenu(menuName = "Settings/Game Settings")]
    public class GameSettings : ScriptableObject 
    {
        [SerializeField, MinValue(1)] private int numberOfHomeworksToFinishGame = 10;

        public int NumberOfHomeworksToFinishGame => numberOfHomeworksToFinishGame;
    }
}