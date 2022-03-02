using Sirenix.OdinInspector;
using UnityEngine;

namespace Scriptables
{
    [CreateAssetMenu(menuName = "Settings/Homework Settings")]
    public class HomeworkSettings : ScriptableObject
    {
        [SerializeField, MinValue(0f), MinMaxSlider(0f, 30f, true)] private Vector2 secondsBeforeHomeworkSpawn = new Vector2(10f, 20f);
        [SerializeField, MinValue(0)] private int maxNumberOfHomeworksInPlay = 3;

        public float MinSecondsBeforeHomeworkSpawn => secondsBeforeHomeworkSpawn.x;
        public float MaxSecondsBeforeHomeworkSpawn => secondsBeforeHomeworkSpawn.y;
        public int MaxNumberOfHomeworksInPlay => maxNumberOfHomeworksInPlay;
    }
}