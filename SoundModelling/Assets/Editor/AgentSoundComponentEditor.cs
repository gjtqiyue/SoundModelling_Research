using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using SoundSystem;

[CustomEditor(typeof(AgentSoundComponent))]
public class AgentSoundComponentEditor : Editor
{
    AgentSoundComponent component;
    int volume;
    float duration;

    private void OnEnable()
    {
        component = target as AgentSoundComponent;
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        GUILayout.Space(10);

        volume = EditorGUILayout.IntField("Volume", volume);
        duration = EditorGUILayout.FloatField("Duration", duration);

        if(GUILayout.Button("Make Sound"))
        {
            component.MakeSound(component.gameObject, component.transform.position, volume, SoundType.Walk, 360, duration);
        }

        GUILayout.Space(10);
    }
}
