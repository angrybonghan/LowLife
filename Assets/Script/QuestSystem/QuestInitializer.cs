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

            Vector3 origin = quest.questCenterPosition;
            Gizmos.color = Color.red;

            Gizmos.DrawRay(origin, Vector3.up * quest.detectUp);
            Gizmos.DrawRay(origin, Vector3.down * quest.detectDown);
            Gizmos.DrawRay(origin, Vector3.left * quest.detectLeft);
            Gizmos.DrawRay(origin, Vector3.right * quest.detectRight);
        }
    }
}