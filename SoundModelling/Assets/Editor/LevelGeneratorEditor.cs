using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(LevelGenerator))]
public class LevelGeneratorEditor : Editor
{
    LevelGenerator generator;

    private void OnEnable()
    {
        generator = target as LevelGenerator;
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        GUILayout.Space(10);
        if (GUILayout.Button("Generate Level"))
        {
            generator.GenerateLevel();
        }
    }
}
