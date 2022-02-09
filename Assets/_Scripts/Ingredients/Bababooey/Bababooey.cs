using System.Collections;
using Systems.Sound;
using UnityEngine;

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
                
                yield return new WaitForSeconds(1f);
            }
        }
    }
}