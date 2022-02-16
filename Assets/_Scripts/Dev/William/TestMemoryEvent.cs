using UnityEngine;

namespace Dev.William
{
    public class TestMemoryEvent : MonoBehaviour
    {
        private void Start()
        {
            TestMemoryEventManager.Instance.OnDoTheThing += ObserveTheThing;
        }

        private void OnDestroy()
        {
            if (TestMemoryEventManager.HasInstance)
                TestMemoryEventManager.Instance.OnDoTheThing -= ObserveTheThing;
        }

        private void ObserveTheThing(int theThing)
        {
            Debug.Log($"{gameObject.name} observed {theThing}");
        }
    }
}