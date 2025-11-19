using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(QuestDataSO))]
public class QuestDataSOEditor : Editor
{
    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        SerializedProperty completeOnDialogueEnd = serializedObject.FindProperty("completeOnDialogueEnd");

        EditorGUILayout.PropertyField(serializedObject.FindProperty("questID"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("questName"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("description"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("achievementID"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("achievementPopup"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("prerequisiteQuestID"));
        EditorGUILayout.PropertyField(completeOnDialogueEnd);

        //completeOnDialogueEnd이 false일 때만 감지 필드 표시
        if (!completeOnDialogueEnd.boolValue)
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("적 감지 설정", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("detectUp"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("detectDown"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("detectLeft"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("detectRight"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("enemyLayer"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("questCenter"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("npcToRescue"));
        }

        serializedObject.ApplyModifiedProperties();
    }
}