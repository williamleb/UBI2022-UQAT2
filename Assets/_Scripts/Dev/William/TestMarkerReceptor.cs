using System;
using System.Collections;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Canvases.Markers
{
    public class TestMarkerReceptor : MonoBehaviour
    {
        [SerializeField, Required] private SpriteMarkerReceptor markerReceptor;

        private void Start()
        {
            StartCoroutine(Yoyo());
        }

        private IEnumerator Yoyo()
        {
            while (true)
            {
                yield return new WaitForSeconds(2f);
                markerReceptor.Activate();
                yield return new WaitForSeconds(10f);
                markerReceptor.Deactivate();
            }
        }
    }
}