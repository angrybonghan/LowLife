using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

/// <summary>
/// QuestDataSO 커스텀 인스펙터.
/// 퀘스트 타입에 따라 필요한 필드만 표시.
/// </summary>
[CustomEditor(typeof(QuestDataSO))]
public class QuestDataSOEditor : Editor
{
    public override void OnInspectorGUI()
    {
        // 대상 QuestDataSO 가져오기
        QuestDataSO questData = (QuestDataSO)target;

        // 기본 정보 표시
        EditorGUILayout.LabelField("기본 정보", EditorStyles.boldLabel);
        questData.questID = EditorGUILayout.TextField("퀘스트 ID", questData.questID);
        questData.questName = EditorGUILayout.TextField("퀘스트 이름", questData.questName);
        questData.description = EditorGUILayout.TextField("퀘스트 설명", questData.description);
        questData.prerequisiteQuestID = EditorGUILayout.TextField("선행 퀘스트 ID", questData.prerequisiteQuestID);

        // 퀘스트 타입 선택
        questData.questType = (QuestType)EditorGUILayout.EnumPopup("퀘스트 타입", questData.questType);

        EditorGUILayout.Space();

        // 타입별 필드 표시
        switch (questData.questType)
        {
            case QuestType.CombatClear:
            case QuestType.CombatCount:
                EditorGUILayout.LabelField("Combat 퀘스트 설정", EditorStyles.boldLabel);
                questData.detectUp = EditorGUILayout.FloatField("감지 위", questData.detectUp);
                questData.detectDown = EditorGUILayout.FloatField("감지 아래", questData.detectDown);
                questData.detectLeft = EditorGUILayout.FloatField("감지 왼쪽", questData.detectLeft);
                questData.detectRight = EditorGUILayout.FloatField("감지 오른쪽", questData.detectRight);
                questData.enemyLayer = LayerMaskField("적 Layer", questData.enemyLayer);
                questData.questCenterPosition = EditorGUILayout.Vector3Field("퀘스트 중심 위치", questData.questCenterPosition);

                if (questData.questType == QuestType.CombatCount)
                {
                    questData.targetKillCount = EditorGUILayout.IntField("목표 처치 수", questData.targetKillCount);
                }
                break;

            case QuestType.Collect:
                EditorGUILayout.LabelField("Collect 퀘스트 설정", EditorStyles.boldLabel);
                questData.requiredItemID = EditorGUILayout.TextField("필요 아이템 ID", questData.requiredItemID);
                questData.requiredItemCount = EditorGUILayout.IntField("필요 아이템 개수", questData.requiredItemCount);
                break;

            case QuestType.Delivery:
                EditorGUILayout.LabelField("Delivery 퀘스트 설정", EditorStyles.boldLabel);
                questData.deliveryTargetNPC = EditorGUILayout.TextField("전달 대상 NPC", questData.deliveryTargetNPC);
                break;

            case QuestType.Explore:
                EditorGUILayout.LabelField("Explore 퀘스트 설정", EditorStyles.boldLabel);
                questData.targetSceneName = EditorGUILayout.TextField("목표 씬 이름", questData.targetSceneName);
                questData.exploreTargetPosition = EditorGUILayout.Vector3Field("목표 위치", questData.exploreTargetPosition);
                questData.exploreRadius = EditorGUILayout.FloatField("도달 반경", questData.exploreRadius);
                break;

            case QuestType.Escort:
                EditorGUILayout.LabelField("Escort 퀘스트 설정", EditorStyles.boldLabel);
                questData.escortTargetSceneName = EditorGUILayout.TextField("호위 목표 씬 이름", questData.escortTargetSceneName);
                questData.escortTargetPosition = EditorGUILayout.Vector3Field("호위 목표 위치", questData.escortTargetPosition);
                questData.escortCompleteRadius = EditorGUILayout.FloatField("호위 완료 반경", questData.escortCompleteRadius);
                break;

            case QuestType.Dialogue:
                EditorGUILayout.LabelField("Dialogue 퀘스트 설정", EditorStyles.boldLabel);
                EditorGUILayout.HelpBox("NPC와 대화하는 퀘스트입니다. 별도 설정 필요 없음.", MessageType.Info);
                break;

            case QuestType.Cutscene:
                EditorGUILayout.LabelField("Cutscene 퀘스트 설정", EditorStyles.boldLabel);
                questData.cutsceneID = EditorGUILayout.TextField("컷씬 ID", questData.cutsceneID);
                EditorGUILayout.HelpBox("지정한 컷씬이 끝나면 자동으로 클리어되는 퀘스트입니다.", MessageType.Info);
                break;
        }

        EditorGUILayout.Space();

        // 변경사항 저장
        if (GUI.changed)
        {
            EditorUtility.SetDirty(questData);
        }
    }

    /// <summary>
    /// LayerMask를 인스펙터에서 선택할 수 있도록 표시
    /// </summary>
    private LayerMask LayerMaskField(string label, LayerMask selected)
    {
        var layers = InternalEditorUtility.layers;
        var layerNumbers = new int[layers.Length];
        for (int i = 0; i < layers.Length; i++)
            layerNumbers[i] = LayerMask.NameToLayer(layers[i]);

        int maskWithoutEmpty = 0;
        for (int i = 0; i < layers.Length; i++)
        {
            if (((1 << layerNumbers[i]) & selected.value) != 0)
                maskWithoutEmpty |= (1 << i);
        }

        maskWithoutEmpty = EditorGUILayout.MaskField(label, maskWithoutEmpty, layers);
        int mask = 0;
        for (int i = 0; i < layers.Length; i++)
        {
            if ((maskWithoutEmpty & (1 << i)) != 0)
                mask |= (1 << layerNumbers[i]);
        }
        selected.value = mask;
        return selected;
    }
}