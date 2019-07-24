using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using SoundSystem;

[CustomEditor(typeof(DiffractionPoint))]
public class DiffractionPointEditor : Editor
{
    DiffractionPoint diffracPoint;

    int currentOverlappingPoint = -1;
    int currentSelectedPoint = -1;

    private void OnEnable()
    {
        diffracPoint = target as DiffractionPoint;
    }

    private void OnSceneGUI()
    {
        Event guiEvent = Event.current;

        //get the position of the mouse
        //guiEvent.mousePosition is gui coordinate, we need to convert it to world position
        Ray mouseRay = HandleUtility.GUIPointToWorldRay(guiEvent.mousePosition);
        float drawPlaneHeight = diffracPoint.transform.position.y;  //user defined height for the plane
        float distToDrawPlane = (drawPlaneHeight - mouseRay.origin.y) / mouseRay.direction.y; //equation: origin.y + dir.y * distance = h
        Vector3 mousePosition = mouseRay.GetPoint(distToDrawPlane);

        if (diffracPoint.edgeVector_1 == Vector3.zero || diffracPoint.edgeVector_2 == Vector3.zero)
        {
            diffracPoint.edgeVector_1 = UtilityMethod.RotateAroundY(30, diffracPoint.transform.forward);
            diffracPoint.edgeVector_2 = UtilityMethod.RotateAroundY(-30, diffracPoint.transform.forward);
        }

        if (guiEvent.type == EventType.Layout)
        {
            HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Keyboard));
        }
        else
        {
            HandleInput(guiEvent, mousePosition);
            DrawHandles(guiEvent, mousePosition);
        }
    }

    private void DrawHandles(Event guiEvent, Vector3 mousePosition)
    {
        Vector3 handlePos_1 = diffracPoint.transform.position + diffracPoint.edgeVector_1 * 5;
        Vector3 handlePos_2 = diffracPoint.transform.position + diffracPoint.edgeVector_2 * 5;

        Handles.color = Color.black;
        Handles.DrawDottedLine(diffracPoint.transform.position, handlePos_1, 3f);
        Handles.DrawDottedLine(diffracPoint.transform.position, handlePos_2, 3f);

        if (Vector3.SqrMagnitude(mousePosition - handlePos_1) < 0.15f || currentSelectedPoint == 1)
        {
            Handles.color = Color.red;
            Handles.DrawSolidDisc(handlePos_1, Vector3.up, 0.5f);
            Handles.color = Color.black;
            Handles.DrawSolidDisc(handlePos_2, Vector3.up, 0.3f);
            currentOverlappingPoint = 1;
        }
        else if (Vector3.SqrMagnitude(mousePosition - handlePos_2) < 0.15f || currentSelectedPoint == 2)
        {
            Handles.color = Color.red;
            Handles.DrawSolidDisc(handlePos_2, Vector3.up, 0.5f);
            Handles.color = Color.black;
            Handles.DrawSolidDisc(handlePos_1, Vector3.up, 0.3f);
            currentOverlappingPoint = 2;
        }
        else
        {
            Handles.color = Color.black;
            Handles.DrawSolidDisc(handlePos_1, Vector3.up, 0.3f);
            Handles.DrawSolidDisc(handlePos_2, Vector3.up, 0.3f);
            currentOverlappingPoint = -1;
        }
        
        Handles.color = Color.gray;
        float angle = Vector3.SignedAngle(diffracPoint.edgeVector_1, diffracPoint.edgeVector_2, Vector3.up);
        if (angle < 0) { angle += 360; }    //change signed angle for ones < 0
        Handles.DrawWireArc(diffracPoint.transform.position, Vector3.up, diffracPoint.edgeVector_1, angle, 5);

        Repaint();
    }

    private void HandleInput(Event guiEvent, Vector3 mousePos)
    {
        if (currentOverlappingPoint > 0 && guiEvent.type == EventType.MouseDown && guiEvent.button == 0 && guiEvent.modifiers == EventModifiers.None)
        {
            currentSelectedPoint = currentOverlappingPoint;
        }

        if (currentSelectedPoint > 0 && guiEvent.type == EventType.MouseDrag && guiEvent.button == 0 && guiEvent.modifiers == EventModifiers.None)
        {
            Vector3 diff = mousePos - diffracPoint.transform.position;
            Vector3 normalized = diff.normalized;

            if (currentSelectedPoint == 1)
            {
                diffracPoint.edgeVector_1 = normalized;
            }
            else
            {
                diffracPoint.edgeVector_2 = normalized;
            }
        }

        if (guiEvent.type == EventType.MouseUp && guiEvent.button == 0)
        {
            currentSelectedPoint = -1;
        }
    }
}
