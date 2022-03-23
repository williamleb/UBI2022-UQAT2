using UnityEngine;

namespace Utilities.Extensions
{
    public static class AnimatorExtensions
    {
        // Taken from https://forum.unity.com/threads/getting-layer-information-from-animation-event.523430/
        public static int GetLayer(this Animator animator, AnimatorStateInfo animInfo)
        {
            for (int i = 0; i < animator.layerCount; i++)
            {
                //Check current state on layer
                var compareAnimEvent = animator.GetCurrentAnimatorStateInfo(i);
                if (animInfo.shortNameHash == compareAnimEvent.shortNameHash)
                {
                    return i;
                }
 
                //Also check the next, since timing is sometimes a bit off
                compareAnimEvent = animator.GetNextAnimatorStateInfo(i);
                if (animInfo.shortNameHash == compareAnimEvent.shortNameHash)
                {
                    return i;
                }
            }
            //Couldn't find
            return -1;
        }
    }
}