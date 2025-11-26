using UnityEngine;
using UnityEditor;

// QuestDataSO의 커스텀 에디터
[CustomEditor(typeof(QuestDataSO))]
public class QuestDataSOEditor : Editor
{
    // Foldout 상태 저장용 변수들
    private bool showCombatSettings = true;
    private bool showDeliverySettings = true;
    private bool showCollectSettings = true;
    private bool showExploreSettings = true;
    private bool showEscortSettings = true;
    private bool showCompletionSettings = true;

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        // 기본 정보 섹션
        DrawSection("퀘스트 기본 정보", new string[]
        {
            "questID", "questName", "description", "prerequisiteQuestID", "questType"
        });

        // questID 유효성 검사
        SerializedProperty questID = serializedObject.FindProperty("questID");
        if (string.IsNullOrEmpty(questID.stringValue))
        {
            EditorGUILayout.HelpBox("퀘스트 ID는 반드시 입력해야 합니다.", MessageType.Warning);
        }

        // 퀘스트 타입에 따라 조건 분기
        SerializedProperty questType = serializedObject.FindProperty("questType");
        QuestType type = (QuestType)questType.enumValueIndex;

        // 퀘스트 타입별 설정 섹션
        switch (type)
        {
            case QuestType.Combat:
                showCombatSettings = EditorGUILayout.Foldout(showCombatSettings, "전투 퀘스트 설정");
                if (showCombatSettings)
                {
                    DrawSection(null, new string[]
                    {
                        "detectUp", "detectDown", "detectLeft", "detectRight",
                        "enemyLayer", "questCenterPosition"
                    });
                }
                break;

            case QuestType.Delivery:
                showDeliverySettings = EditorGUILayout.Foldout(showDeliverySettings, "아이템 전달 퀘스트 설정");
                if (showDeliverySettings)
                {
                    DrawSection(null, new string[]
                    {
                        "requiredItemID", "deliveryTargetNPC"
                    });
                }
                break;

            case QuestType.Collect:
                showCollectSettings = EditorGUILayout.Foldout(showCollectSettings, "아이템 수집 퀘스트 설정");
                if (showCollectSettings)
                {
                    DrawSection(null, new string[]
                    {
                        "requiredItemID", "requiredItemCount"
                    });
                }
                break;

            case QuestType.Explore:
                showExploreSettings = EditorGUILayout.Foldout(showExploreSettings, "탐험 퀘스트 설정");
                if (showExploreSettings)
                {
                    DrawSection(null, new string[]
                    {
                        "targetSceneName", "exploreTargetPosition", "exploreRadius"
                    });
                }
                break;

            case QuestType.Escort:
                showEscortSettings = EditorGUILayout.Foldout(showEscortSettings, "호위 퀘스트 설정");
                if (showEscortSettings)
                {
                    DrawSection(null, new string[]
                    {
                        "escortTargetSceneName", "escortTargetPosition", "escortCompleteRadius"
                    });
                }
                break;
        }

        // 완료 시 연출 섹션
        showCompletionSettings = EditorGUILayout.Foldout(showCompletionSettings, "퀘스트 완료 시 연출");
        if (showCompletionSettings)
        {
            DrawSection(null, new string[]
            {
                "achievementID", "achievementPopup"
            });
        }

        // 시각적 구분선
        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

        serializedObject.ApplyModifiedProperties();
    }

    // 섹션 헤더와 필드 묶음 출력 함수
    private void DrawSection(string title, string[] propertyNames)
    {
        EditorGUILayout.Space();
        if (!string.IsNullOrEmpty(title))
        {
            EditorGUILayout.LabelField(title, EditorStyles.boldLabel);
        }

        foreach (var propName in propertyNames)
        {
            SerializedProperty prop = serializedObject.FindProperty(propName);
            if (prop != null)
            {
                EditorGUILayout.PropertyField(prop);
            }
        }
    }
}