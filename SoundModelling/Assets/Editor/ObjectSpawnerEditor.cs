using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(ObjectSpawner))]
public class ObjectSpawnerEditor : Editor
{
    ObjectSpawner spawner;

    int objectNum;

    float drawPlaneHeight;

    GameObject currentSelectedObj;

    Vector3 dragStartPoint;

    private void OnEnable()
    {
        spawner = target as ObjectSpawner;
        Tools.hidden = true;
    }

    private void OnDisable()
    {
        Tools.hidden = false;
    }

    private void OnSceneGUI()
    {
        Event guiEvent = Event.current;

        if (guiEvent.type == EventType.Layout)
        {
            HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));
        }
        else
        {
            HandleInput(guiEvent);
        }
    }

    private void HandleInput(Event guiEvent)
    {
        //get the position of the mouse
        //guiEvent.mousePosition is gui coordinate, we need to convert it to world position
        Ray mouseRay = HandleUtility.GUIPointToWorldRay(guiEvent.mousePosition);
        float distToDrawPlane = (drawPlaneHeight - mouseRay.origin.y) / mouseRay.direction.y;
        Vector3 mousePosition = mouseRay.GetPoint(distToDrawPlane);

        if (guiEvent.type == EventType.MouseDown && guiEvent.button == 0 && guiEvent.modifiers == EventModifiers.Shift)
        {
            HandleLeftShiftMouseDown(mousePosition);
        }

        if (guiEvent.type == EventType.MouseDown && guiEvent.button == 0 && guiEvent.modifiers == EventModifiers.None)    // left mouse down
        {
            HandleLeftMouseDown(mousePosition);
        }

        if (guiEvent.type == EventType.MouseUp && guiEvent.button == 0)    // left mouse up
        {
            HandleLeftMouseUp(mousePosition);
        }

        if (guiEvent.type == EventType.MouseDrag && guiEvent.button == 0 && guiEvent.modifiers == EventModifiers.None)
        {
            //user defined spawn place
            HandleLeftMouseDrag(mousePosition);
        }

        if (guiEvent.type == EventType.MouseDrag && guiEvent.button == 0 && guiEvent.modifiers == EventModifiers.Shift)
        {
            //spawn in a row
            HandleLeftShiftMouseDrag(mousePosition);
            
        }

        Selection.activeGameObject = spawner.gameObject;
    }

    private void HandleLeftShiftMouseDown(Vector3 mousePos)
    {
        Debug.Log("shift down");
        dragStartPoint = SpawnObject(mousePos);
    }

    private void HandleLeftMouseDown(Vector3 mousePos)
    {
        SpawnObject(mousePos);
    }

    private void HandleLeftMouseUp(Vector3 mousePos)
    {
        currentSelectedObj = null;
    }

    private void HandleLeftMouseDrag(Vector3 mousePos)
    {
        if (currentSelectedObj)
        {
            if (spawner.snapToGrid)
            {
                SnapToGrid(currentSelectedObj.transform, mousePos);
            }
            else
            {
                currentSelectedObj.transform.position = mousePos;
            }
        }
    }

    private void HandleLeftShiftMouseDrag(Vector3 mousePos)
    {
        Debug.Log("shift drag");
        float length = currentSelectedObj.transform.localScale.z;
        float width = currentSelectedObj.transform.localScale.x;
        float height = currentSelectedObj.transform.localScale.y;
        if (Mathf.Abs(mousePos.x - dragStartPoint.x) >= width || Mathf.Abs(mousePos.y - dragStartPoint.y) >= height || Mathf.Abs(mousePos.z - dragStartPoint.z) >= length)
        {
            dragStartPoint = SpawnObject(mousePos);
        }
    }

    private Vector3 SpawnObject(Vector3 mousePos)
    {
        if (spawner.selectedObjectIndex > -1)
        {
            GameObject gameObject = (GameObject)PrefabUtility.InstantiatePrefab(spawner.objects[spawner.selectedObjectIndex]);
            Undo.RegisterCreatedObjectUndo(gameObject, "Spawn gameobject");

            currentSelectedObj = gameObject;

            if (spawner.snapToGrid)
            {
                return SnapToGrid(currentSelectedObj.transform, mousePos);
            }
            else
            {
                currentSelectedObj.transform.position = mousePos;
                return mousePos;
            }
        }

        return Vector3.zero;
    }

    private Vector3 SnapToGrid(Transform trans, Vector3 mousePos)
    {

        float length = trans.localScale.z;
        float width = trans.localScale.x;
        float height = trans.localScale.y;
        Debug.Log(width);

        float gridX = (Mathf.RoundToInt(mousePos.x / width)) * width;
        float gridY = (Mathf.RoundToInt(mousePos.y / height)) * height;
        float gridZ = (Mathf.RoundToInt(mousePos.z / length)) * length;

        trans.position = new Vector3(gridX, gridY, gridZ);

        return trans.position;
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        drawPlaneHeight = EditorGUILayout.FloatField("Draw Height", drawPlaneHeight);

        spawner.objectList = EditorGUILayout.Foldout(spawner.objectList, "Object List");
        if (spawner.objectList)
        {
            GUILayout.BeginHorizontal();
            objectNum = EditorGUILayout.IntField("Object count", objectNum);
            GUILayout.EndHorizontal();

            if (spawner.objects.Count > 0)
            {
                objectNum = spawner.objects.Count;
            }

            if (objectNum > 0)
            {
                while (spawner.objects.Count < objectNum)
                {
                    spawner.objects.Add(null);
                }

                int deleteObjectIndex = -1;
                for (int i=0; i<objectNum; i++)
                {
                    GUILayout.BeginHorizontal();
                    spawner.objects[i] = (GameObject)EditorGUILayout.ObjectField(spawner.objects[i], typeof(GameObject), false);

                    GUI.enabled = i != spawner.selectedObjectIndex;    //gray out the button if the shape is selected
                    if (GUILayout.Button("Select"))
                    {
                        spawner.selectedObjectIndex = i;
                    }
                    GUI.enabled = true;

                    if (GUILayout.Button("Delete"))
                    {
                        deleteObjectIndex = i;
                    }
                    GUILayout.EndHorizontal();
                }

                if (deleteObjectIndex > 0)
                {
                    spawner.objects.RemoveAt(deleteObjectIndex);
                }
            }
        }
    }
}
