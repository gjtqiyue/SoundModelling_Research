using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(BezierCurve))]
public class SplineToolEditor : Editor
{
    private static Color[] modeColors = {
        Color.white,
        Color.yellow,
        Color.cyan
    };

    private BezierCurve spline;
    private Transform handleTransform;
    private Quaternion handleRotation;

    private int selectedIndex = -1;

    private int lineSteps = 10;

    private void OnEnable()
    {
        spline = target as BezierCurve;

        Tools.hidden = true;
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        EditorGUI.BeginChangeCheck();
        bool loop = EditorGUILayout.Toggle("Loop", spline.Loop);
        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(spline, "Toggle Loop");
            EditorUtility.SetDirty(spline);
            spline.Loop = loop;
        }

        if (selectedIndex > 0 && selectedIndex < spline.PointCount)
        {
            DrawInspector();
        }
        if (GUILayout.Button("Add curve"))
        {
            spline.AddCurve();
            SceneView.RepaintAll();
        }
        if (GUILayout.Button("Reset curve"))
        {
            spline.Reset();
            SceneView.RepaintAll();
        }
    }

    private void DrawInspector()
    {
        GUILayout.Label("Selected point");

        EditorGUI.BeginChangeCheck();
        Vector3 p = EditorGUILayout.Vector3Field("Position", spline.GetControlPoint(selectedIndex));
        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(spline, "Move Point");
            EditorUtility.SetDirty(spline);
            spline.SetControlPoint(selectedIndex, handleTransform.InverseTransformPoint(p));
        }

        EditorGUI.BeginChangeCheck();
        CurveMode m = (CurveMode)EditorGUILayout.EnumPopup("Mode", spline.GetControlPointMode(selectedIndex));
        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(spline, "Change Mode");
            EditorUtility.SetDirty(spline);
            spline.SetControlPointMode(selectedIndex, m);
        }
    }

    private void OnSceneGUI()
    {
        handleTransform = spline.transform;
        //The rotation should be adjusted accordingly based on unity's rotation mode (local or global)
        handleRotation = Tools.pivotRotation == PivotRotation.Local ?
            handleTransform.rotation : Quaternion.identity;

        //Handle all the points first

        for (int i = 0; i < spline.PointCount - 1; i++)
        {
            Vector3 p0 = UpdatePoint(i);
            Vector3 p1 = UpdatePoint(i + 1);

            Handles.color = Color.gray;
            Handles.DrawLine(p0, p1);
        }

        //Draw the line between, move along the line with variable t (0, 1)
        DrawDirections();
        for (int i = 0; i < spline.PointCount - 3; i+=3)
        {
            Handles.DrawBezier(spline.GetControlPoint(i), spline.GetControlPoint(i+3), spline.GetControlPoint(i+1), spline.GetControlPoint(i+2), Color.white, null, 2f);
        }
    }

    private void DrawDirections()
    {
        Vector3 lineStart = spline.GetBezierPoint(0f);
        Handles.color = Color.green;
        Handles.DrawLine(lineStart, lineStart + spline.GetDirection(0));
        float steps = lineSteps * spline.CurveCount;
        for (int i = 1; i <= steps; i++)
        {
            Vector3 lineEnd = spline.GetBezierPoint(i / (float)steps);
            Handles.color = Color.green;
            Handles.DrawLine(lineEnd, lineEnd + spline.GetDirection(i / (float)steps));
            lineStart = lineEnd;
        }
    }

    public float handleSize = 0.04f;
    public float pickSize = 0.06f;

    private Vector3 UpdatePoint(int index)
    {
        Vector3 p = spline.GetControlPoint(index);

        float size = HandleUtility.GetHandleSize(p);    //get the fixed point size for the scene

        if (index == 0)
        {
            size *= 2f;
        }

        Handles.color = modeColors[(int)spline.GetControlPointMode(index)];
        if (Handles.Button(p, handleRotation, size * handleSize, size * pickSize, Handles.DotHandleCap))
        {
            selectedIndex = index;
            Repaint();
        }

        if (selectedIndex == index)
        {
            EditorGUI.BeginChangeCheck();
            p = Handles.DoPositionHandle(p, handleRotation);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(spline, "Move Point");
                EditorUtility.SetDirty(spline);
                spline.SetControlPoint(index, handleTransform.InverseTransformPoint(p));
            }
            
        }
        return p;
    }

    
}
