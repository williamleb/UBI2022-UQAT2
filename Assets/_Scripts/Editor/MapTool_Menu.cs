using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace TechArt.Tools
{
    public class MapTool_Menu
    {
        [MenuItem("Homework Arena Tools/Maps/MapTool")]
        public static void MapTool_Main()
        {
            // Launch the editor window
            Debug.Log("Starting map tool...");
            MapTool_Editor.LaunchEditor();
        }
    }
}
