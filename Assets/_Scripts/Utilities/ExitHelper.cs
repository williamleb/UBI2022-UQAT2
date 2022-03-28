// ReSharper disable once RedundantUsingDirective
using UnityEngine;

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