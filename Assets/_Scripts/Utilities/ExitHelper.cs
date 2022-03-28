#if !UNITY_EDITOR
using UnityEngine;
#endif

namespace Utilities
{
    public static class ExitHelper
    {
        public static void ExitGame()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}