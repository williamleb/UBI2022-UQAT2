using UnityEngine;

namespace Canvases.Markers
{
    public class ThrowChargeMarkerReceptor : MarkerReceptor<ThrowChargeMarker>
    {
        [SerializeField] private float chargeAmount = 0f;

        public float ChargeAmount
        {
            get => chargeAmount;
            set
            {
                if (CurrentMarker)
                    CurrentMarker.ChargeAmount = chargeAmount;
                chargeAmount = value;
            }
        }
        
        protected override void OnActivated()
        {
            CurrentMarker.ChargeAmount = chargeAmount;
        }
    }
}