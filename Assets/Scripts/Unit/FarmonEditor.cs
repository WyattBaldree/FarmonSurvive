using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Farmon), true)]
public class FarmonEditor : Editor
{
    GUIStyle centeredStyle;

    SerializedProperty grit;
    SerializedProperty power;
    SerializedProperty reflex;
    SerializedProperty focus;
    SerializedProperty speed;

    float statButtonWidth = 20;
    float statLabelValueWidth = 15;
    float statLabelNameWidth = 50;

    void OnEnable()
    {
        grit = serializedObject.FindProperty("grit");
        power = serializedObject.FindProperty("power");
        reflex = serializedObject.FindProperty("reflex");
        focus = serializedObject.FindProperty("focus");
        speed = serializedObject.FindProperty("speed");
    }

    public override void OnInspectorGUI()
    {
        base.DrawDefaultInspector();

        centeredStyle = GUI.skin.GetStyle("Label");
        centeredStyle.alignment = TextAnchor.UpperCenter;

        GUILayout.Space(20f);
        GUILayout.Label("Stats", EditorStyles.boldLabel);

        CreateStatBlock("Grit", grit, (u) => u.Grit, (u, i) => u.Grit = i); 
        CreateStatBlock("Power", power, (u) => u.Power, (u, i) => u.Power = i);
        CreateStatBlock("Reflex", reflex, (u) => u.Reflex, (u, i) => u.Reflex = i);
        CreateStatBlock("Focus", focus, (u) => u.Focus, (u, i) => u.Focus = i);
        CreateStatBlock("Speed", speed, (u) => u.Speed, (u, i) => u.Speed = i);

        // Apply changes to the serializedProperty - always do this at the end of OnInspectorGUI.

        if(Application.isPlaying) serializedObject.Update();
        serializedObject.ApplyModifiedProperties();

        if (Application.isPlaying)
        {
            if (GUILayout.Button("LevelUp"))
            {
                ((Farmon)target).LevelUp();
            }
            if (GUILayout.Button("Die"))
            {
                ((Farmon)target).Die();
            }
        }
    }

    private void CreateStatBlock(string statName, SerializedProperty statSerializedProperty, Func<Farmon, int> statPropertyGet, Action<Farmon, int> statPropertySet)
    {
        Farmon unit = (Farmon)target;

        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        if (statSerializedProperty.prefabOverride)
        {
            GUI.color = Color.cyan;
        }
        GUILayout.Label(statName, EditorStyles.boldLabel, GUILayout.Width(statLabelNameWidth));
        GUI.color = Color.white;
        GUILayout.Space(10);
        if (GUILayout.Button("-", GUILayout.Width(statButtonWidth)))
        {
            statPropertySet(unit, statPropertyGet(unit) - 1);
            statSerializedProperty.intValue = statPropertyGet(unit);
        }
        GUILayout.Label(statPropertyGet(unit).ToString(), centeredStyle, GUILayout.Width(statLabelValueWidth));
        if (GUILayout.Button("+", GUILayout.Width(statButtonWidth)))
        {
            statPropertySet(unit, statPropertyGet(unit) + 1);
            statSerializedProperty.intValue = statPropertyGet(unit);
        }
        GUILayout.EndHorizontal();
    }
}
