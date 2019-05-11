using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(ShapeCreator))]
public class ShapeEditor : Editor
{
    ShapeCreator shapeCreator;
    SelectionInfo selectionInfo;

    bool shapeChangedSinceLastRepaint = false;

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        string helpMessage = "Left click to add points.\nShift-click on point to delete.\nShift-left click on empty space to create a new shape.";
        EditorGUILayout.HelpBox(helpMessage, MessageType.Info);

        int shapeDeleteIndex = -1;
        shapeCreator.showShapesList = EditorGUILayout.Foldout(shapeCreator.showShapesList, "Show Shapes List");
        if (shapeCreator.showShapesList)
        {
            for (int i = 0; i < shapeCreator.shapes.Count; i++)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label("Shape " + (i + 1));

                GUI.enabled = i != selectionInfo.selectedShapeIndex;    //gray out the button if the shape is selected
                if (GUILayout.Button("Select"))
                {
                    selectionInfo.selectedShapeIndex = i;
                }
                GUI.enabled = true;

                if (GUILayout.Button("Delete"))
                {
                    shapeDeleteIndex = i;
                }
                GUILayout.EndHorizontal();
            }
        }

        if (shapeDeleteIndex != -1)
        {
            Undo.RecordObject(shapeCreator, "Delete shape");
            shapeCreator.shapes.RemoveAt(shapeDeleteIndex);
            selectionInfo.selectedShapeIndex = Mathf.Clamp(selectionInfo.selectedShapeIndex, 0, shapeCreator.shapes.Count - 1);
        }

        if (GUI.changed)
        {
            shapeChangedSinceLastRepaint = true;
            SceneView.RepaintAll();
        }
    }

    private void OnSceneGUI()
    {
        //called when there is a input event in the scene
        Event guiEvent = Event.current;

        if (guiEvent.type == EventType.Repaint)
        {
            Draw();
            shapeChangedSinceLastRepaint = false;
        }
        else if (guiEvent.type == EventType.Layout)
        {
            HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));
        }
        else
        {
            HandleInput(guiEvent);
            if (shapeChangedSinceLastRepaint)
            {
                HandleUtility.Repaint();
            }
        }
    }

    private void CreateNewShape()
    {
        Undo.RecordObject(shapeCreator, "Create shape");
        shapeCreator.shapes.Add(new Shape());
        selectionInfo.selectedShapeIndex = shapeCreator.shapes.Count - 1;   //default we want to select the new shape
    }

    private void CreateNewPoint(Vector3 mousePos)
    {
        bool mouseIsOverSelectedShape = selectionInfo.mouseOverShapeIndex == selectionInfo.selectedShapeIndex;
        int newPointIndex = (selectionInfo.mouseIsOverLine && mouseIsOverSelectedShape) ? selectionInfo.lineIndex + 1 : SelectedShape.points.Count;
        Undo.RecordObject(shapeCreator, "Add point");
        SelectedShape.points.Insert(newPointIndex, mousePos);
        selectionInfo.pointIndex = newPointIndex;   //after add a point, set the selected point to this point
        selectionInfo.mouseOverShapeIndex = selectionInfo.selectedShapeIndex;
        shapeChangedSinceLastRepaint = true;

        SelectPointUnderMouse();
    }

    private void DeletePointUnderMouse()
    {
        Undo.RecordObject(shapeCreator, "Delete point");
        SelectedShape.points.RemoveAt(selectionInfo.pointIndex);
        selectionInfo.mouseIsOverPoint = false;
        selectionInfo.pointIsSelected = false;
        shapeChangedSinceLastRepaint = true;
    }

    private void HandleInput(Event guiEvent)
    {
        //get the position of the mouse
        //guiEvent.mousePosition is gui coordinate, we need to convert it to world position
        Ray mouseRay = HandleUtility.GUIPointToWorldRay(guiEvent.mousePosition);
        float drawPlaneHeight = 0;  //user defined height for the plane
        float distToDrawPlane = (drawPlaneHeight - mouseRay.origin.y) / mouseRay.direction.y;
        Vector3 mousePosition = mouseRay.GetPoint(distToDrawPlane);

        if (guiEvent.type == EventType.MouseDown && guiEvent.button == 0 && guiEvent.modifiers == EventModifiers.Shift)    // left mouse down
        {
            HandleLeftShiftMouseDown(mousePosition);
        }

        if (guiEvent.type == EventType.MouseDown && guiEvent.button == 0 && guiEvent.modifiers == EventModifiers.None)    // left mouse down
        {
            HandleLeftMouseDown(mousePosition);
        }

        if (guiEvent.type == EventType.MouseUp && guiEvent.button == 0)    // left mouse down
        {
            HandleLeftMouseUp(mousePosition);
        }

        if (guiEvent.type == EventType.MouseDrag && guiEvent.button == 0 && guiEvent.modifiers == EventModifiers.None)    // left mouse down
        {
            HandleLeftMouseDrag(mousePosition);
        }

        if (!selectionInfo.pointIsSelected)
        {
            UpdateMouseOverInfo(mousePosition);
        }
    }

    private void SelectShapeUnderMouse()
    {
        if (selectionInfo.mouseOverShapeIndex != -1)
        {
            selectionInfo.selectedShapeIndex = selectionInfo.mouseOverShapeIndex;
            shapeChangedSinceLastRepaint = true;
        }
    }

    private void SelectPointUnderMouse()
    {
        selectionInfo.mouseIsOverPoint = true;
        selectionInfo.pointIsSelected = true;
        selectionInfo.mouseIsOverLine = false;
        selectionInfo.lineIndex = -1;

        selectionInfo.positionStartDrag = SelectedShape.points[selectionInfo.pointIndex];
    }

    // handle the input to create new shape
    private void HandleLeftShiftMouseDown(Vector3 mousePos)
    {
        if (selectionInfo.mouseIsOverPoint)
        {
            SelectShapeUnderMouse();
            DeletePointUnderMouse();
        }
        else
        {
            CreateNewShape();
            CreateNewPoint(mousePos);
        }
    }

    private void HandleLeftMouseDown(Vector3 mousePos)
    {
        if (shapeCreator.shapes.Count == 0)
        {
            CreateNewShape();
        }

        SelectShapeUnderMouse();

        if (selectionInfo.mouseIsOverPoint)
        {
            SelectPointUnderMouse();
        }
        else
        {
            CreateNewPoint(mousePos);
        }
    }

    private void HandleLeftMouseUp(Vector3 mousePos)
    {
        if (selectionInfo.pointIsSelected)
        {
            SelectedShape.points[selectionInfo.pointIndex] = selectionInfo.positionStartDrag;
            Undo.RecordObject(shapeCreator, "Move point");
            SelectedShape.points[selectionInfo.pointIndex] = mousePos;

            selectionInfo.pointIsSelected = false;
            selectionInfo.pointIndex = -1;
            shapeChangedSinceLastRepaint = true;
        }
    }

    private void HandleLeftMouseDrag(Vector3 mousePos)
    {
        if (selectionInfo.pointIsSelected)
        {
            SelectedShape.points[selectionInfo.pointIndex] = mousePos;
            shapeChangedSinceLastRepaint = true;
        }
    }

    private void UpdateMouseOverInfo(Vector3 mousePos)
    {
        int mouseOverPointIndex = -1;
        int mouseOverShapeIndex = -1;
        for (int obstacleIndex = 0; obstacleIndex < shapeCreator.shapes.Count; obstacleIndex++)
        {
            Shape currentObstacle = shapeCreator.shapes[obstacleIndex];
            for (int i = 0; i < currentObstacle.points.Count; i++)
            {
                if (Vector3.Distance(mousePos, currentObstacle.points[i]) < shapeCreator.handleRadius)
                {
                    mouseOverPointIndex = i;
                    mouseOverShapeIndex = obstacleIndex;
                    break;
                }
            }
        }

        if (mouseOverPointIndex != selectionInfo.pointIndex || mouseOverShapeIndex != selectionInfo.mouseOverShapeIndex)
        {
            selectionInfo.pointIndex = mouseOverPointIndex;
            selectionInfo.mouseIsOverPoint = mouseOverPointIndex != -1;

            selectionInfo.mouseOverShapeIndex = mouseOverShapeIndex;

            shapeChangedSinceLastRepaint = true;
        }

        if (selectionInfo.mouseIsOverPoint)
        {
            selectionInfo.mouseIsOverLine = false;
            selectionInfo.lineIndex = -1;
        }
        else
        {
            int mouseOverLineIndex = -1;
            float closetLineDist = shapeCreator.handleRadius;
            for (int obstacleIndex = 0; obstacleIndex < shapeCreator.shapes.Count; obstacleIndex++)
            {
                Shape currentObstacle = shapeCreator.shapes[obstacleIndex];
                for (int i = 0; i < currentObstacle.points.Count; i++)
                {
                    Vector3 nextPoint = currentObstacle.points[(i + 1) % currentObstacle.points.Count];
                    float distFromMouseToLine = HandleUtility.DistancePointToLineSegment(mousePos.ToXZ(), currentObstacle.points[i].ToXZ(), nextPoint.ToXZ());
                    if (distFromMouseToLine < closetLineDist)
                    {
                        closetLineDist = distFromMouseToLine;
                        mouseOverLineIndex = i;
                        mouseOverShapeIndex = obstacleIndex;
                    }
                }
            }

            if (selectionInfo.lineIndex != mouseOverLineIndex || mouseOverShapeIndex != selectionInfo.mouseOverShapeIndex)
            {
                selectionInfo.lineIndex = mouseOverLineIndex;
                selectionInfo.mouseIsOverLine = mouseOverLineIndex != -1;

                selectionInfo.mouseOverShapeIndex = mouseOverShapeIndex;

                shapeChangedSinceLastRepaint = true;
            }
        }
    }

    private void Draw()
    {
        for (int obstacleIndex = 0; obstacleIndex < shapeCreator.shapes.Count; obstacleIndex++)
        {
            Shape obsToDraw = shapeCreator.shapes[obstacleIndex];
            bool shapeIsSelected = obstacleIndex == selectionInfo.selectedShapeIndex;
            bool mouseIsOverShape = obstacleIndex == selectionInfo.mouseOverShapeIndex;
            Color deselectedShapeColor = Color.gray;    //draw everything gray if not currently selected

            for (int i = 0; i < obsToDraw.points.Count; i++)
            {
                Vector3 nextPoint = obsToDraw.points[(i + 1) % obsToDraw.points.Count];
                if (i == selectionInfo.lineIndex && mouseIsOverShape)
                {
                    Handles.color = Color.red;
                    Handles.DrawLine(obsToDraw.points[i], nextPoint);
                }
                else
                {
                    Handles.color = shapeIsSelected ? Color.black : deselectedShapeColor;
                    Handles.DrawDottedLine(obsToDraw.points[i], nextPoint, 4);
                }
                //change color to red if the mouse is selecting a point
                if (i == selectionInfo.pointIndex && mouseIsOverShape)
                {
                    Handles.color = selectionInfo.pointIsSelected ? Color.black : Color.red;
                }
                else
                {
                    Handles.color = shapeIsSelected ? Color.white : deselectedShapeColor;
                }
                Handles.DrawSolidDisc(obsToDraw.points[i], Vector3.up, .5f);
            }
        }
        //Handles.color = Color.white;
        //Handles.DrawAAConvexPolygon(shapeCreator.points.ToArray());

        if (shapeChangedSinceLastRepaint)
        {
            shapeCreator.UpdateMeshDisplay();
        }
    }

    private void OnEnable()
    {
        shapeChangedSinceLastRepaint = true;
        shapeCreator = target as ShapeCreator;
        selectionInfo = new SelectionInfo();
        Undo.undoRedoPerformed += OnUndoOrRedo;
        Tools.hidden = true;
    }

    private void OnDisable()
    {
        Undo.undoRedoPerformed -= OnUndoOrRedo;
        Tools.hidden = false;
    }

    private void OnUndoOrRedo()
    {
        if (selectionInfo.selectedShapeIndex >= shapeCreator.shapes.Count || selectionInfo.selectedShapeIndex == -1)
        {
            selectionInfo.selectedShapeIndex = shapeCreator.shapes.Count - 1;
        }
        shapeChangedSinceLastRepaint = true;
    }

    Shape SelectedShape
    {
        get
        {
            return shapeCreator.shapes[selectionInfo.selectedShapeIndex];
        }
    }

    public class SelectionInfo
    {
        public int selectedShapeIndex;
        public int mouseOverShapeIndex;

        public int pointIndex = -1;
        public int lineIndex = -1;
        public bool mouseIsOverPoint;
        public bool pointIsSelected;
        public bool mouseIsOverLine;
        public Vector3 positionStartDrag;
    }
}
