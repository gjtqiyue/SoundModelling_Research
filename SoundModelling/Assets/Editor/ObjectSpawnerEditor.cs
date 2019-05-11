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

    private void OnEnable()
    {
        spawner = target as ObjectSpawner;
    }

    private void OnSceneGUI()
    {
        Event guiEvent = Event.current;

        //get the position of the mouse
        //guiEvent.mousePosition is gui coordinate, we need to convert it to world position
        Ray mouseRay = HandleUtility.GUIPointToWorldRay(guiEvent.mousePosition);
        float distToDrawPlane = (drawPlaneHeight - mouseRay.origin.y) / mouseRay.direction.y;
        Vector3 mousePosition = mouseRay.GetPoint(distToDrawPlane);

        if (guiEvent.type == EventType.MouseDown && guiEvent.button == 0 && guiEvent.modifiers == EventModifiers.None)    // left mouse down
        {
            HandleLeftMouseDown(mousePosition);
            
        }

        Selection.activeGameObject = spawner.gameObject;
    }

    private void HandleLeftMouseDown(Vector3 mousePos)
    {
        if (spawner.selectedObjectIndex > -1)
        {
            GameObject gameObject = (GameObject)PrefabUtility.InstantiatePrefab(spawner.objects[spawner.selectedObjectIndex]);
            gameObject.transform.position = mousePos;

            Undo.RegisterCreatedObjectUndo(gameObject, "Spawn gameobject");
        }
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
