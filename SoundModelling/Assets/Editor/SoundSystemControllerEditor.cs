using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using SoundSystem;

[CustomEditor(typeof(SystemController))]
public class SoundSystemControllerEditor : Editor
{
    SystemController systemController;

    bool displayMap;
    bool showDebuggingOptions;
    bool showSoundMapOptions;

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        EditorGUILayout.Space();

        showSoundMapOptions = EditorGUILayout.Foldout(showSoundMapOptions, "Sound Map");
        if (showSoundMapOptions)
        {
            systemController.startPoint = EditorGUILayout.Vector3Field("Start Point", systemController.startPoint);

            systemController.width = EditorGUILayout.IntField("Width", systemController.width);
            systemController.length = EditorGUILayout.IntField("Length", systemController.length);

            systemController.resolution = EditorGUILayout.Vector2Field("Resolution", systemController.resolution);

            systemController.quadPrefab = (GameObject)EditorGUILayout.ObjectField("Unit Prefab", systemController.quadPrefab, typeof(GameObject), false);
        }

        GUILayout.Space(1);
        showDebuggingOptions = EditorGUILayout.Foldout(showDebuggingOptions, "Debugging Option");
        if (showDebuggingOptions)
        {
            systemController.drawPointDistribution = GUILayout.Toggle(systemController.drawPointDistribution, "Point Distribution");
        }
    }

    private void OnEnable()
    {
        systemController = target as SystemController;
    }
}
