using UnityEngine;
using UnityEditor;

// QuestDataSO의 커스텀 에디터
[CustomEditor(typeof(QuestDataSO))]
public class QuestDataSOEditor : Editor
{
    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        // 기본 정보 섹션
        DrawSection("퀘스트 기본 정보", new string[]
        {
            "questID", "questName", "description", "prerequisiteQuestID", "questType"
        });

        // 퀘스트 타입에 따라 조건 분기
        SerializedProperty questType = serializedObject.FindProperty("questType");
        QuestType type = (QuestType)questType.enumValueIndex;

        // 전투 퀘스트 설정
        if (type == QuestType.Combat)
        {
            DrawSection("전투 퀘스트 설정", new string[]
            {
                "detectUp", "detectDown", "detectLeft", "detectRight", "enemyLayer", "questCenterPosition"
            });
        }

        // 아이템 퀘스트 설정
        if (type == QuestType.Delivery)
        {
            DrawSection("아이템 퀘스트 설정", new string[]
            {
                "requiredItemID"
            });
        }

        // 완료 시 연출
        DrawSection("퀘스트 완료 시 연출", new string[]
        {
            "npcToRescue", "achievementID", "achievementPopup"
        });

        serializedObject.ApplyModifiedProperties();
    }

    // 섹션 헤더와 필드 묶음 출력 함수
    private void DrawSection(string title, string[] propertyNames)
    {
        EditorGUILayout.Space();
        EditorGUILayout.LabelField(title, EditorStyles.boldLabel);

        foreach (var propName in propertyNames)
        {
            SerializedProperty prop = serializedObject.FindProperty(propName);
            if (prop != null)
                EditorGUILayout.PropertyField(prop);
        }
    }
}