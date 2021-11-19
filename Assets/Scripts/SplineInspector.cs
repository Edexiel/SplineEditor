using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Animations;


[CustomEditor(typeof(Spline))]
public class SplineMenu : Editor
{
    private Spline _spline;
    private Transform pointTransform;
    private Quaternion pointRotation;

    // private Color SelectionColor = Color.red;
    // private float SelectionSize = 2f;

    public int SelectedIndex = -1;


    private void OnSceneGUI()
    {
        _spline = target as Spline;

        pointTransform = _spline.transform;
        pointRotation = Tools.pivotRotation == PivotRotation.Local ? pointTransform.rotation : Quaternion.identity;

        for (int i = 3; i < _spline.points.Count; i += 3)
        {
            Handles.color = Color.white;
            Vector3 p0 = ShowPoint(i - 3);
            Handles.color = Color.blue;
            Vector3 p1 = ShowPoint(i - 2);
            Handles.color = Color.red;
            Vector3 p2 = ShowPoint(i - 1);
            Handles.color = Color.white;
            Vector3 p3 = ShowPoint(i);


            //draw control points of curve
            Handles.color = Color.white;
            Handles.DrawLine(p0, p1);
            Handles.DrawLine(p2, p3);
            // Handles.DrawLine(p1, p2);
        }

        Tools.hidden = !_spline.GameObjectHandle;
    }


    public override void OnInspectorGUI()
    {
        _spline = target as Spline;

        // EditorGUILayout.LabelField("Selection");
        // SelectionColor = EditorGUILayout.ColorField("Color", SelectionColor);
        // SelectionSize = EditorGUILayout.FloatField("Size", SelectionSize);
        // EditorGUILayout.Space();

        DrawDefaultInspector();

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Add Curve"))
        {
            Undo.RecordObject(_spline, "Add Curve");
            _spline.AddCurve();
            EditorUtility.SetDirty(_spline);
        }

        if (GUILayout.Button("Delete last curve"))
        {
            Undo.RecordObject(_spline, "Delete Curve");
            _spline.DeleteLastCurve();
            EditorUtility.SetDirty(_spline);
        }

        GUILayout.EndHorizontal();
    }

    private Vector3 ShowPoint(int index)
    {
        bool isNode = index % 3 == 0;

        Vector3 point = pointTransform.TransformPoint(_spline.points[index]);

        EditorGUI.BeginChangeCheck();

        //is selected
        if (SelectedIndex == index)
        {
            point = Handles.DoPositionHandle(point, pointRotation);
        }
        else
        {

            float size = HandleUtility.GetHandleSize(point);
            if (Handles.Button(point, pointRotation, size * 0.08f, size * 0.15f, Handles.DotHandleCap))
            {
                SelectedIndex = index;
            }
        }

        if (EditorGUI.EndChangeCheck())
        {
            // Set list dirty to regenerate point list

            //move tangents with point
            Vector3 movement = pointTransform.InverseTransformPoint(point) - _spline.points[index];


            Undo.RecordObject(_spline, "Move spline point");
            EditorUtility.SetDirty(_spline);
            _spline.points[index] = pointTransform.InverseTransformPoint(point);

            if (isNode)
            {
                if (index + 1 <= _spline.points.Count - 1)
                {
                    _spline.points[index + 1] += movement;
                }

                if (index - 1 >= 0)
                {
                    _spline.points[index - 1] += movement;
                }
            }


            if (_spline.smoothSpline)
                _spline.SmoothCurve(index);
        }

        _spline.isDirty = true;

        return point;
    }
}