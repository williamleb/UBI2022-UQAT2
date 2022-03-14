using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Ingredients.Homework;
using Systems.Settings;
using Utilities.Extensions;
using Utilities.Singleton;

namespace Units.AI.Actions
{
    public class AITaskOrchestrator : Singleton<AITaskOrchestrator>
    {
        private readonly List<Homework> homeworkSubscriptions = new List<Homework>();

        private AISettings settings;

        private void Start()
        {
            settings = SettingsSystem.AISettings;
            
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

        private void OnHomeworkStateChanged(Homework homework)
        {
            if (!homework.IsInWorld)
                return;

            AssignTask(homework);
        }

        public void TransferHomeworkTask(Homework homework)
        {
            AssignTask(homework);
        }

        private async void AssignTask(Homework homework)
        {
            var secondsToNoticeHomeworks = UnityEngine.Random.Range(settings.MinSecondsToNoticeHomework, settings.MaxSecondsToNoticeHomework);
            await Task.Delay(TimeSpan.FromSeconds(secondsToNoticeHomeworks));
            
            var nearestStudent = GetNearestStudentThatCanReceiveTasksFrom(homework);
            if (!nearestStudent)
                return;

            if (!homework.IsInWorld)
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