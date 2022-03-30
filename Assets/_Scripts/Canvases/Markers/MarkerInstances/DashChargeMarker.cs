using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

public class DashChargeMarker : MonoBehaviour
{
    [SerializeField, Required] private Image imageToFill;

    public float ChargeAmount
    {
        set => imageToFill.fillAmount = value;
    }
}
