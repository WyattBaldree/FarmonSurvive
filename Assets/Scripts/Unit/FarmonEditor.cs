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

    SerializedProperty gritBonus;
    SerializedProperty powerBonus;
    SerializedProperty agilityBonus;
    SerializedProperty focusBonus;
    SerializedProperty luckBonus;

    float statButtonWidth = 20;
    float statLabelValueWidth = 15;
    float statLabelNameWidth = 50;

    string saveName;

    void OnEnable()
    {
        gritBonus = serializedObject.FindProperty("gritBonus");
        powerBonus = serializedObject.FindProperty("powerBonus");
        agilityBonus = serializedObject.FindProperty("agilityBonus");
        focusBonus = serializedObject.FindProperty("focusBonus");
        luckBonus = serializedObject.FindProperty("luckBonus");
    }

    public override void OnInspectorGUI()
    {
        Farmon unit = (Farmon)target;

        base.DrawDefaultInspector();

        centeredStyle = GUI.skin.GetStyle("Label");
        centeredStyle.alignment = TextAnchor.UpperCenter;

        GUILayout.Space(20f);
        GUILayout.Label("Stats", EditorStyles.boldLabel);

        CreateStatBlock("Grit",unit.GritBase, gritBonus, (u) => u.GritBonus, (u, i) => u.GritBonus = i); 
        CreateStatBlock("Power", unit.PowerBase, powerBonus, (u) => u.PowerBonus, (u, i) => u.PowerBonus = i);
        CreateStatBlock("Agility", unit.AgilityBase, agilityBonus, (u) => u.AgilityBonus, (u, i) => u.AgilityBonus = i);
        CreateStatBlock("Focus", unit.FocusBase, focusBonus, (u) => u.FocusBonus, (u, i) => u.FocusBonus = i);
        CreateStatBlock("Luck", unit.LuckBase, luckBonus, (u) => u.LuckBonus, (u, i) => u.LuckBonus = i);

        // Apply changes to the serializedProperty - always do this at the end of OnInspectorGUI.

        if (Application.isPlaying)
        {
            if (GUILayout.Button("Give Jump Perk"))
            {
                ((Farmon)target).AddPerk(new PerkJump());
            }
            if (GUILayout.Button("LevelUp"))
            {
                ((Farmon)target).LevelUp();
            }
            if (GUILayout.Button("Die"))
            {
                ((Farmon)target).Die();
            }

            if (GUILayout.Button("Save Player"))
            {
                SaveController.SavePlayer();
            }
            if (GUILayout.Button("Load Player"))
            {
                SaveController.LoadPlayer();
            }

            if (GUILayout.Button("Save Player Farmon"))
            {
                Player.instance.FarmonSquadSaveIds[0] = SaveController.SaveFarmonPlayer(unit);
            }
            if (GUILayout.Button("Load Player Farmon"))
            {
                if (Player.instance.FarmonSquadSaveIds[0] != 0)
                {
                    SaveController.LoadFarmonPlayer(Player.instance.FarmonSquadSaveIds[0]);
                }
            }
        }
        saveName = GUILayout.TextField(saveName);
        if (GUILayout.Button("Save"))
        {
            SaveController.SaveFarmon(unit, saveName);
        }
        if (GUILayout.Button("Load"))
        {
            Farmon.ConstructFarmon(SaveController.LoadFarmon(saveName));
        }

        if (Application.isPlaying) serializedObject.Update();
        serializedObject.ApplyModifiedProperties();
    }

    private void CreateStatBlock(string statName, int baseStat, SerializedProperty statSerializedProperty, Func<Farmon, int> statPropertyGet, Action<Farmon, int> statPropertySet)
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
            if(statPropertyGet(unit) > 0)
            {
                statPropertySet(unit, statPropertyGet(unit) - 1);
                statSerializedProperty.intValue = statPropertyGet(unit);
            }
        }
        GUILayout.Label((baseStat + statPropertyGet(unit)).ToString(), centeredStyle, GUILayout.Width(statLabelValueWidth));
        if (GUILayout.Button("+", GUILayout.Width(statButtonWidth)))
        {
            int potentialStatValue = unit.GritBase + statPropertyGet(unit) + 1;
            if (potentialStatValue <= Farmon.StatMax)
            {
                statPropertySet(unit, statPropertyGet(unit) + 1);
                statSerializedProperty.intValue = statPropertyGet(unit);
            }
        }
        GUILayout.EndHorizontal();
    }
}
