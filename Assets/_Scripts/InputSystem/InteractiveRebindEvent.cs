using System;
using UnityEngine.Events;
using UnityEngine.InputSystem;

namespace InputSystem
{
    [Serializable]
    public class InteractiveRebindEvent : UnityEvent<RebindActionUI, InputActionRebindingExtensions.RebindingOperation>
    {
    }
}