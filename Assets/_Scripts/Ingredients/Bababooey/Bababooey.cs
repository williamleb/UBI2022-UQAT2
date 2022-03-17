using System.Collections;
using Systems.Sound;
using UnityEngine;
using Utilities;

namespace Ingredients.Bababooey
{
    public class Bababooey : MonoBehaviour
    {
        private Coroutine bababooeyCoroutine;
        
        private void Start()
        {
            bababooeyCoroutine = StartCoroutine(BababooeyRoutine());
        }

        private void OnDestroy()
        {
            if (bababooeyCoroutine != null)
                StopCoroutine(bababooeyCoroutine);
        }

        private IEnumerator BababooeyRoutine()
        {
            while (true)
            {
                SoundSystem.Instance.PlayBababooeySound();
                
                yield return Helpers.GetWait(1f);
            }
        }
    }
}