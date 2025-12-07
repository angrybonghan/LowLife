using UnityEngine;

/// <summary>
/// 씬 시작 시 EnemyZone만 연결.
/// - 퀘스트 등록은 QuestManager에서 한 번만 담당
/// - 씬마다 Combat 퀘스트 Zone만 연결
/// </summary>
public class QuestInitializer : MonoBehaviour
{
    [Header("Combat 퀘스트 Zone들")]
    public EnemyZone[] enemyZones;

    private void Awake()
    {
        foreach (var zone in enemyZones)
        {
            if (zone == null || zone.questData == null) continue;

            // QuestManager에서 해당 퀘스트 가져오기
            var quest = QuestManager.Instance.GetQuestData(zone.questData.questID);
            if (quest != null)
            {
                zone.questData = quest;
                Debug.Log($"[QuestInitializer] Combat 퀘스트 {quest.questName} -> EnemyZone 연결 완료");
            }
        }
    }
}