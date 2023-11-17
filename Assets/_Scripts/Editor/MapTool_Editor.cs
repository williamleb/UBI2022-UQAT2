using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using Fusion.Editor;
using System.Linq;
using BehaviorDesigner.Runtime.Tasks.Unity.UnityVector3;
using GluonGui.Dialog;
using System.Linq.Expressions;
using UnityEngine.Timeline;
using System;
using ICSharpCode.NRefactory.Ast;
using System.Security.Policy;
using Scriptables;
using UnityEditor.VersionControl;
using System.Reflection;

namespace TechArt.Tools
{
    public class MapTool_Editor : EditorWindow
    {
        #region Variables
        public GameObject sourceObject;

        int currentSelectionCount = 0;
        public bool[] enableOverridePosition = { false, true, true, true };

        // UI
        public Vector2 scrollPosition;
        public bool togglePO = false;
        public bool[] togglePOxyz = { true, true, true };
        #endregion

        #region Builtin Methods
        public static void LaunchEditor()
        {
            var editorWin = GetWindow<MapTool_Editor>();
            editorWin.Show();
        }

        private void OnGUI()
        {
            GetSelection();

            EditorGUILayout.BeginVertical();
            EditorGUILayout.Space();

            EditorGUILayout.LabelField("Selection Count: " + currentSelectionCount.ToString(), EditorStyles.boldLabel);
            EditorGUILayout.Space();

            sourceObject = (GameObject)EditorGUILayout.ObjectField("Replace Object: ", sourceObject, typeof(GameObject), false);
            if(GUILayout.Button("Replace Selected Objects", GUILayout.ExpandWidth(true), GUILayout.Height(40)))
            {
                ReplaceSelected();
            }
            EditorGUILayout.Space();

            EditorGUILayout.EndVertical();

            togglePO = EditorGUILayout.BeginToggleGroup("Override Position", togglePO);
            {
                togglePOxyz[0] = EditorGUILayout.Toggle("x", togglePOxyz[0]);
                togglePOxyz[1] = EditorGUILayout.Toggle("y", togglePOxyz[1]);
                togglePOxyz[2] = EditorGUILayout.Toggle("z", togglePOxyz[2]);
            }
            EditorGUILayout.EndToggleGroup();

            enableOverridePosition = new bool[] { togglePO, togglePOxyz[0], togglePOxyz[1], togglePOxyz[2] };

            // DEBUG OUTPUT
            EditorGUILayout.BeginVertical();
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Output");
            EditorGUILayout.EndVertical();

            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.ExpandWidth(true), GUILayout.Height(200));

            GUILayout.Label(DebugSelection(), GUI.skin.textArea);
            
            EditorGUILayout.EndScrollView();

            Repaint();
        }
        #endregion

        #region Custom Methods
        void GetSelection()
        {
            currentSelectionCount = 0;
            currentSelectionCount = Selection.gameObjects.Length;
        }

        Bounds GetRenderBounds(GameObject target)
        {
            Bounds bounds = new Bounds(Vector3.zero, Vector3.zero);
            Renderer render = target.GetComponent<Renderer>();
            if (render != null)
            {
                return render.bounds;
            }
            return bounds;
        }

        Bounds GetBounds(GameObject target)
        {
            Bounds bounds;
            Renderer childRender;
            bounds = GetRenderBounds(target);
            if (bounds.extents.x == 0)
            {
                bounds = new Bounds(target.transform.position, Vector3.zero);
                foreach (Transform child in target.transform)
                {
                    childRender = child.GetComponent<Renderer>();
                    if (childRender)
                    {
                        bounds.Encapsulate(childRender.bounds);
                    }
                    else
                    {
                        bounds.Encapsulate(GetBounds(child.gameObject));
                    }
                }
            }
            return bounds;
        }

        Vector3 GetOffset(Bounds source)
        {
            Vector3 result = source.extents;

            Vector3 multiplier;
            multiplier.x = Math.Sign(source.center.x);
            multiplier.y = Math.Sign(source.center.y);
            multiplier.z = Math.Sign(source.center.z);
            multiplier *= -1;

            result.Scale(multiplier);

            return result;
        }

        GameObject InstantiateRelativeToTransform(GameObject target, Vector3 pos, Quaternion rot)
        {
            Bounds sourceBounds = GetBounds(target);

            Vector3 pos_override = pos;
            pos += GetOffset(sourceBounds);

            if (enableOverridePosition[0])
            {
                if (enableOverridePosition[1]) { pos.x = pos_override.x; }
                if (enableOverridePosition[2]) { pos.y = pos_override.y; }
                if (enableOverridePosition[3]) { pos.z = pos_override.z; }
            }

            GameObject newObject = (GameObject)PrefabUtility.InstantiatePrefab(target);
            newObject.transform.position = pos;
            // Rotate object while keeping position in the center
            newObject.transform.RotateAround(pos, Vector3.right, rot.eulerAngles.x);
            newObject.transform.RotateAround(pos, Vector3.up, rot.eulerAngles.y);
            newObject.transform.RotateAround(pos, Vector3.forward, rot.eulerAngles.z);
            newObject.transform.localScale = target.transform.localScale;

            return newObject;
        }

        GameObject InstantiateRelativeToTransform(GameObject target, Vector3 pos, Quaternion rot, Vector3 pos_override)
        {
            Bounds sourceBounds = GetBounds(target);

            pos += GetOffset(sourceBounds);

            if (enableOverridePosition[0])
            {
                if (enableOverridePosition[1]) { pos.x = pos_override.x; }
                if (enableOverridePosition[2]) { pos.y = pos_override.y; }
                if (enableOverridePosition[3]) { pos.z = pos_override.z; }
            }

            GameObject newObject = (GameObject)PrefabUtility.InstantiatePrefab(target);
            newObject.transform.position = pos;
            // Rotate object while keeping position in the center
            newObject.transform.RotateAround(pos, Vector3.right, rot.eulerAngles.x);
            newObject.transform.RotateAround(pos, Vector3.up, rot.eulerAngles.y);
            newObject.transform.RotateAround(pos, Vector3.forward, rot.eulerAngles.z);
            newObject.transform.localScale = target.transform.localScale;

            return newObject;
        }

        int[] RussianBounds(Bounds outer, Bounds inner)
        {
            Vector3 outerSize = outer.extents * 2;
            Vector3 innerSize = inner.extents * 2;

            Vector3 diff; // = VDivide(outerSize, innerSize, true);
            diff.x = (float)Math.Abs(Math.Floor(outerSize.x / innerSize.x));
            diff.y = (float)Math.Abs(Math.Floor(outerSize.y / innerSize.y));
            diff.z = (float)Math.Abs(Math.Floor(outerSize.z / innerSize.z));
            if (diff.y <= 0) { diff.y = 1; }

            int[] result = new int[3] { 
                (int)diff.x,
                (int)diff.y,
                (int)diff.z 
            };

            return result;
        }

        void ReplaceSelected()
        {
            // Check for selection count
            if (currentSelectionCount == 0)
            {
                DialogWarning("At least one object needs to be selected!");
                return;
            }

            // Check for target
            if (!sourceObject)
            {
                DialogWarning("No replacement object selected!");
                return;
            }

            Bounds sourceBounds = GetBounds(sourceObject);

            // Replace Objects
            GameObject[] selectedObjects = Selection.gameObjects;
            for(int i = 0; i < selectedObjects.Length; i++)
            {
                GameObject zoneParent = new GameObject(sourceObject.name + "_Zone" + i.ToString().PadLeft(2, (char)0));
                Vector3 newPos = selectedObjects[i].transform.position;
                Quaternion newRot = selectedObjects[i].transform.rotation;
                Vector3 oldPos = zoneParent.transform.position;
                Quaternion oldRot = zoneParent.transform.rotation;

                selectedObjects[i].transform.rotation = zoneParent.transform.rotation; // set temporary transform for correct reading of bounds

                GameObject zone = selectedObjects[i];
                Bounds zoneBounds = GetBounds(zone);
                int[] count = RussianBounds(zoneBounds, sourceBounds);

                Vector3 sourceSize = sourceBounds.extents * 2;

                Debug.Log(count[0].ToString() + " | " + count[1].ToString() + " | " + count[2].ToString());
                for (int xindex = 0; xindex < count[0]; xindex++)
                {
                    for (int yindex = 0; yindex < count[1]; yindex++)
                    {
                        for (int zindex = 0; zindex < count[2]; zindex++)
                        {
                            Debug.Log(xindex.ToString() + " | " + yindex.ToString() + " | " + zindex.ToString());
                            float px = sourceSize.x * xindex;
                            float py = sourceSize.y * yindex;
                            float pz = sourceSize.z * zindex;
                            Vector3 p = oldPos + new Vector3(px, py, pz);
                            GameObject newPrefab = InstantiateRelativeToTransform(sourceObject, p, oldRot);
                            newPrefab.transform.SetParent(zoneParent.transform);
                        }
                    }
                }
                zoneParent.transform.position = newPos;
                zoneParent.transform.rotation = newRot;

                //DestroyImmediate(selectedObjects[i]); // Get rid of the original (Destructive)
                //selectedObjects[i].SetActive(false); // Get rid of the original (Safe)
            }

        }
        #endregion

        #region Feedback Methods
        string DebugSelection()
        {
            // Check for selection count
            if (currentSelectionCount == 0)
            {
                return "";
            }

            // Check for target
            if (!sourceObject)
            {
                return "";
            }

            // Generate debug text
            string res;
            Bounds sourceBounds = GetBounds(sourceObject);
            Vector3 sourceOffset = GetOffset(sourceBounds);
            res = FormatPreview("Source", sourceObject.name);
            res += FormatPreview("Type", sourceObject.GetType().ToString());
            res += FormatPreview("Bounds", sourceBounds.ToString());
            res += FormatPreview("Offset", sourceOffset.ToString());
            res += FormatPreview("Position", sourceObject.transform.position.ToString());
            res += "\n";
            GameObject[] selectedObjects = Selection.gameObjects;
            for (int i = 0; i < selectedObjects.Length; i++)
            {
                res += FormatPreview("Index", i.ToString());
                res += FormatPreview("Name", selectedObjects[i].name);
                res += FormatPreview("Type", selectedObjects[i].GetType().ToString());
                Bounds b = GetBounds(selectedObjects[i]);
                res += FormatPreview("Bounds", b.ToString());
                Transform t = selectedObjects[i].transform;
                res += FormatPreview("Position", t.position.ToString());
                int[] count = RussianBounds(b, sourceBounds);
                res += FormatPreview("Russian", count[0].ToString() + " | " + count[1].ToString() + " | " + count[2].ToString());

                res += "\n";
            }

            return res;
        }

        /*void TESTDEBUG()
        {
            GameObject[] selectedObjects = Selection.gameObjects;
            for (int i = 0; i < selectedObjects.Length; i++)
            {
                Get
            }
        }*/

        void DialogError(string message)
        {
            Debug.LogError(message);
            EditorUtility.DisplayDialog("Error", message, "Close");
        }

        void DialogInfo(string message)
        {
            Debug.Log(message);
            EditorUtility.DisplayDialog("Info", message, "Ok");
        }

        void DialogWarning(string message)
        {
            Debug.LogWarning(message);
            EditorUtility.DisplayDialog("Warning", message, "Close");
        }
        #endregion

        string FormatPreview(string name, string value)
        {
            return name + ": " + value + "\n";
        }
    }
}