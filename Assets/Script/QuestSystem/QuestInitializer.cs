using UnityEngine;
using System.Collections.Generic;

// 퀘스트 등록 및 감지 범위 시각화
public class QuestInitializer : MonoBehaviour
{
    public List<QuestDataSO> questsToRegister;

    private void Start()
    {
        foreach (var quest in questsToRegister)
        {
            QuestManager.Instance.AddToActiveQuests(quest);
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (questsToRegister == null) return;

        foreach (var quest in questsToRegister)
        {
            if (quest.questType != QuestType.Combat) continue;

            Vector3 center = quest.questCenterPosition;

            Vector3 size = new Vector3(
                quest.detectLeft + quest.detectRight,
                quest.detectUp + quest.detectDown,
                1f
            );

            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(center, size);
        }
    }
}