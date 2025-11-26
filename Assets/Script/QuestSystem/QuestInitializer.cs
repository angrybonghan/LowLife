using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 씬 시작 시 퀘스트들을 QuestManager에 등록하고,
/// Combat 퀘스트라면 EnemyZone과 연결.
/// </summary>
public class QuestInitializer : MonoBehaviour
{
    public List<QuestDataSO> questsToRegister; // 이 씬에서 사용할 퀘스트들
    public EnemyZone[] enemyZones;             // 씬에 배치된 EnemyZone들

    private void Start()
    {
        foreach (var quest in questsToRegister)
        {
            QuestManager.Instance.AddToActiveQuests(quest);

            foreach (var zone in enemyZones)
            {
                if (zone.questID == quest.questID)
                {
                    zone.questData = quest;
                    Debug.Log($"[QuestInitializer] EnemyZone {zone.name}에 {quest.questID} 연결 완료");
                }
            }
        }
    }

    // Scene 뷰에서 Combat 퀘스트 감지 범위를 시각화
    private void OnDrawGizmosSelected()
    {
        if (questsToRegister == null) return;

        foreach (var quest in questsToRegister)
        {
            if (quest.questType != QuestType.Combat) continue;

            Vector3 center = quest.questCenterPosition;
            Vector3 size = new Vector3(quest.detectLeft + quest.detectRight, quest.detectUp + quest.detectDown, 1f);

            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(center, size);
        }
    }
}