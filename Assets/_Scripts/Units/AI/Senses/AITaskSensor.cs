using Ingredients.Homework;
using UnityEngine;

namespace Units.AI
{
    public class AITaskSensor : MonoBehaviour
    {
        private Homework homeworkTask = null;

        public bool HasHomeworkTask => homeworkTask != null;
        public bool CanReceiveTask { get; set; } = true;
        
        public void GiveHomeworkTask(Homework homework)
        {
            homeworkTask = homework;
        }

        public Homework TakeHomeworkTask()
        {
            var task = homeworkTask;
            homeworkTask = null;
            return task;
        }
    }
}