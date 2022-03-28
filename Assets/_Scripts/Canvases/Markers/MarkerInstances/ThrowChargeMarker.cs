using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

namespace Canvases.Markers
{
    public class ThrowChargeMarker : Marker
    {
        [SerializeField, Required] private Image imageToFill;

        public float ChargeAmount
        {
            set => imageToFill.fillAmount = value;
        }
    }
}