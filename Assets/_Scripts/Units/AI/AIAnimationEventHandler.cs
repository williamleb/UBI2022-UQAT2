using JetBrains.Annotations;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Units.AI
{
    public class AIAnimationEventHandler : MonoBehaviour
    {
        [SerializeField, Required] private AIEntity aiEntity;

        [UsedImplicitly]
        private void AnimationEvent_GetUp(AnimationEvent _)
        {
            aiEntity.IsUpAnimEvent();
        }
    }
}