using System;
using UnityEngine.Events;

namespace InputSystem
{
    [Serializable]
    public class UpdateBindingUIEvent : UnityEvent<RebindActionUI, string, string, string>
    {
    }
}