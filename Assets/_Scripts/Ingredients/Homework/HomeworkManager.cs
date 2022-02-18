using System.Collections.Generic;
using Utilities.Singleton;

namespace Ingredients.Homework
{
    public class HomeworkManager : Singleton<HomeworkManager>
    {
        // The homework spawning logic will change in the future
        // We could also reuse homeworks instead of spawning/despawning them

        private Dictionary<int, Homework> homeworks = new Dictionary<int, Homework>();

        public void RegisterHomework(Homework homework)
        {
            homeworks.Add(homework.HomeworkId, homework);
        }

        public void UnregisterHomework(Homework homework)
        {
            homeworks.Remove(homework.HomeworkId);
        }

        public Homework GetHomework(int homeworkId)
        {
            if (!homeworks.TryGetValue(homeworkId, out var homework))
                return null;

            return homework;
        }
    }
}