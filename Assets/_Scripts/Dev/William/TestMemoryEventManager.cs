using System;
using System.Collections;
using UnityEngine;
using Utilities.Event;
using Utilities.Singleton;

namespace Dev.William
{
    public class TestMemoryEventManager : Singleton<TestMemoryEventManager>
    {
        [NonSerialized] public MemoryEvent<int> OnDoTheThing;
        
        [SerializeField] private GameObject testPrefab;
        private int theThing = 1;
        
        private void Start()
        {
            StartCoroutine(SpawnTest());
        }

        private IEnumerator SpawnTest()
        {
            while (true)
            {
                var theGameObject = Instantiate(testPrefab);
                theGameObject.name = $"Hello{theThing}";
                
                OnDoTheThing.InvokeWithMemory(theThing);
                ++theThing;
                
                yield return new WaitForSeconds(3f);
            }
        }
    }
}