using UnityEngine;
using System.Collections.Generic;

// 에디터에서 등록한 ScriptableObject 퀘스트들을 QuestManager에 등록
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
            if (quest.questCenter == null) continue;

            Vector3 origin = quest.questCenter.position;
            Gizmos.color = Color.red;

            Gizmos.DrawRay(origin, Vector3.up * quest.detectUp);
            Gizmos.DrawRay(origin, Vector3.down * quest.detectDown);
            Gizmos.DrawRay(origin, Vector3.left * quest.detectLeft);
            Gizmos.DrawRay(origin, Vector3.right * quest.detectRight);
        }
    }
}