using System.Collections.Generic;
using Ingredients.Homework;
using Utilities.Extensions;
using Utilities.Singleton;

namespace Units.AI
{
    public class AITaskOrchestrator : Singleton<AITaskOrchestrator>
    {
        private readonly List<Homework> homeworkSubscriptions = new List<Homework>();

        private void Start()
        {
            if (HomeworkManager.HasInstance)
                HomeworkManager.Instance.OnHomeworkRegistered += RegisterToHomeworkStateChanged;
        }

        private void RegisterToHomeworkStateChanged(Homework homework)
        {
            homework.EventOnStateChanged += OnHomeworkStateChanged;
            homeworkSubscriptions.Add(homework);
        }

        protected override void OnDestroy()
        {
            if (HomeworkManager.HasInstance)
                HomeworkManager.Instance.OnHomeworkRegistered -= RegisterToHomeworkStateChanged;

            foreach (var homework in homeworkSubscriptions)
            {
                homework.EventOnStateChanged -= OnHomeworkStateChanged;
            }
            
            base.OnDestroy();
        }

        private void OnHomeworkStateChanged(Homework homework, Homework.State newState)
        {
            if (newState != Homework.State.InWorld)
                return;

            var nearestStudent = GetNearestStudentThatCanReceiveTasksFrom(homework);
            if (!nearestStudent)
                return;

            nearestStudent.TaskSensor.GiveHomeworkTask(homework);
        }

        private AIEntity GetNearestStudentThatCanReceiveTasksFrom(Homework homework)
        {
            if (!AIManager.HasInstance)
                return null;
            
            var minDistance = float.MaxValue;
            AIEntity minStudent = null;

            foreach (var student in AIManager.Instance.Students)
            {
                if (!student.TaskSensor || !student.TaskSensor.CanReceiveTask)
                    continue;
                
                var distanceWithStudent = homework.transform.position.SqrDistanceWith(student.transform.position);
                if (distanceWithStudent < minDistance)
                {
                    minDistance = distanceWithStudent;
                    minStudent = student;
                }
            }

            return minStudent;
        }
    }
}