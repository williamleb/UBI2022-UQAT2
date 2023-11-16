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

            sourceObject = (GameObject)EditorGUILayout.ObjectField("Replace Object: ", sourceObject, typeof(GameObject), true);
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

        Vector3 OverridePosition(Vector3 target, Vector3 value)
        {
            if (enableOverridePosition[0])
            {
                if (enableOverridePosition[1]) { target.x = value.x; }
                if (enableOverridePosition[2]) { target.y = value.y; }
                if (enableOverridePosition[3]) { target.z = value.z; }
            }

            return target;
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

        GameObject InstantiateRelativeToTransform(Transform origin, GameObject target)
        {
            Bounds sourceBounds = GetBounds(target);

            Vector3 pos = origin.position + GetOffset(sourceBounds);
            Quaternion rot = origin.rotation;

            pos = OverridePosition(pos, origin.position);

            GameObject newObject = Instantiate(target);
            newObject.transform.position = pos;
            // Rotate object while keeping position in the center
            newObject.transform.RotateAround(origin.position, Vector3.right, rot.eulerAngles.x);
            newObject.transform.RotateAround(origin.position, Vector3.up, rot.eulerAngles.y);
            newObject.transform.RotateAround(origin.position, Vector3.forward, rot.eulerAngles.z);
            newObject.transform.localScale = target.transform.localScale;

            return newObject;
        }

        void InstantiateRelativeToTransform(int[] count, GameObject zone, GameObject target) 
        {
            Transform origin = zone.transform;
            Vector3 old_position = origin.position;
            Quaternion old_rotation = origin.rotation;
            Vector3 old_scale = origin.localScale;

            Transform t = origin;
            Vector3 p = t.position;
            Vector3 size = GetBounds(target).extents * 2;
            Vector3 zoneSize = GetBounds(zone).extents;
            Debug.Log(count[0].ToString() + " | " + count[1].ToString() + " | " + count[2].ToString());
            for (int xindex = 0; xindex < count[0]; xindex++)
            {
                for (int yindex = 0; yindex < count[1]; yindex++)
                {
                    for (int zindex = 0; zindex < count[2]; zindex++)
                    {
                        Debug.Log(xindex.ToString() + " | " + yindex.ToString() + " | " + zindex.ToString());
                        float tx = size.x * xindex - zoneSize.x;
                        float ty = size.y * yindex - zoneSize.y;
                        float tz = size.z * zindex - zoneSize.z;
                        t.position = new Vector3(tx, ty, tz) + p;
                        InstantiateRelativeToTransform(t, target);
                    }
                }
            }
            // Set back to original transform, keeps moving for some reason??
            origin.position = old_position;
            origin.rotation = old_rotation;
            origin.localScale = old_scale;
        }

        static Vector3 VDivide(Vector3 dividend, Vector3 divisor, bool isFloor = false)
        {
            Vector3 result;

            result.x = dividend.x / divisor.x;
            result.y = dividend.y / divisor.y;
            result.z = dividend.z / divisor.z;

            if (isFloor)
            {
                result = VFloor(result);
            }

            return result;
        }
        
        static Vector3 VFloor(Vector3 target)
        {
            target.x = (float)Math.Floor(target.x);
            target.y = (float)Math.Floor(target.y);
            target.z = (float)Math.Floor(target.z);
            return target;
        }

        int[] RussianBounds(Bounds outer, Bounds inner)
        {
            int[] result = new int[3];

            Vector3 outerSize = outer.extents * 2;
            Vector3 innerSize = inner.extents * 2;

            Vector3 diff = VDivide(outerSize, innerSize, true);

            result = new int[] { 
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
                GameObject zone = selectedObjects[i];
                Bounds zoneBounds = GetBounds(zone);
                Transform zoneTransform = zone.transform;
                int[] count = RussianBounds(zoneBounds, sourceBounds);

                Transform t = zoneTransform;
                Vector3 pos = zoneTransform.position;
                Quaternion rot = zoneTransform.rotation;
                Vector3 scale = zoneTransform.localScale;

                Vector3 sourceSize = sourceBounds.extents * 2;
                Vector3 zoneSize = zoneBounds.extents * (float)0.5;
                Debug.Log(count[0].ToString() + " | " + count[1].ToString() + " | " + count[2].ToString());
                for (int xindex = 0; xindex < count[0]; xindex++)
                {
                    for (int yindex = 0; yindex < count[1]; yindex++)
                    {
                        for (int zindex = 0; zindex < count[2]; zindex++)
                        {
                            Debug.Log(xindex.ToString() + " | " + yindex.ToString() + " | " + zindex.ToString());
                            float tx = sourceSize.x * xindex - zoneSize.x;
                            float ty = sourceSize.y * yindex - zoneSize.y;
                            float tz = sourceSize.z * zindex - zoneSize.z - 1;
                            t.position = pos + new Vector3(tx, ty, tz);
                            InstantiateRelativeToTransform(t, sourceObject);
                        }
                    }
                }
                // Set back to original transform, keeps moving for some reason??
                zoneTransform.position = pos;
                zoneTransform.rotation = rot;
                zoneTransform.localScale = scale;
                //GameObject newObject = InstantiateRelativeToTransform(current.transform, sourceObject);

                //DestroyImmediate(selectedObjects[i]);
                //selectedObjects[i].SetActive(false);
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