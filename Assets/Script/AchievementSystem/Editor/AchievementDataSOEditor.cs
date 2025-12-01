using UnityEngine;
using UnityEditor;

/// <summary>
/// 업적 데이터 인스펙터 커스텀 에디터
/// - 조건 타입에 맞는 필드만 노출해서 실수를 줄이고 가독성을 높임
/// </summary>
[CustomEditor(typeof(AchievementDataSO))]
public class AchievementDataSOEditor : Editor
{
    public override void OnInspectorGUI()
    {
        var ach = (AchievementDataSO)target;

        // 기본 정보
        EditorGUILayout.LabelField("업적 기본 정보", EditorStyles.boldLabel);
        ach.achievementID = EditorGUILayout.TextField("Achievement ID", ach.achievementID);
        ach.title = EditorGUILayout.TextField("Title", ach.title);
        ach.description = EditorGUILayout.TextField("Description", ach.description);
        ach.rewardTitle = EditorGUILayout.TextField("Reward Title", ach.rewardTitle);

        EditorGUILayout.Space();

        // 조건 타입
        EditorGUILayout.LabelField("조건 설정", EditorStyles.boldLabel);
        ach.type = (AchievementType)EditorGUILayout.EnumPopup("Type", ach.type);

        // 타입별 조건 필드
        switch (ach.type)
        {
            case AchievementType.KillEnemy:
                ach.targetEnemyType = EditorGUILayout.TextField("Target Enemy Type", ach.targetEnemyType);
                ach.targetCount = EditorGUILayout.IntField("Target Count", ach.targetCount);
                break;

            case AchievementType.CollectItem:
                ach.targetItemID = EditorGUILayout.TextField("Target Item ID", ach.targetItemID);
                ach.targetCount = EditorGUILayout.IntField("Target Count", ach.targetCount);
                break;

            case AchievementType.ParrySuccess:
            case AchievementType.BlockSuccess:
            case AchievementType.TeleportUse:
            case AchievementType.AttackSuccess:
            case AchievementType.DiscoverSecretRoom:
                ach.targetCount = EditorGUILayout.IntField("Target Count", ach.targetCount);
                break;

            case AchievementType.CompleteQuest:
                ach.targetQuestID = EditorGUILayout.TextField("Target Quest ID", ach.targetQuestID);
                break;

            case AchievementType.EnterScene:
                ach.targetSceneName = EditorGUILayout.TextField("Target Scene Name", ach.targetSceneName);
                break;

            case AchievementType.DiscoverNPC:
            case AchievementType.ViewEnding:
                EditorGUILayout.HelpBox("이 업적은 특정 이벤트 발생 시 즉시 달성됩니다.", MessageType.Info);
                break;
        }

        EditorGUILayout.Space();

        // 진행 상태(읽기 전용 안내)
        EditorGUILayout.LabelField("진행 상태 (런타임)", EditorStyles.boldLabel);
        using (new EditorGUI.DisabledScope(true))
        {
            EditorGUILayout.Toggle("Is Unlocked", ach.isUnlocked);
            EditorGUILayout.IntField("Current Count", ach.currentCount);
        }

        if (GUI.changed)
        {
            EditorUtility.SetDirty(ach);
        }
    }
}