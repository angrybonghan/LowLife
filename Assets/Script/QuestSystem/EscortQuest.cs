using UnityEngine;
using UnityEngine.SceneManagement;

public class EscortQuest : MonoBehaviour
{
    public string questID;
    public Transform escortNPC; // 씬에 있는 NPC Transform

    private void Update()
    {
        QuestDataSO quest = QuestManager.Instance.GetQuestData(questID);
        if (quest == null || quest.questType != QuestType.Escort) return;
        if (QuestManager.Instance.GetQuestState(questID) != QuestState.InProgress) return;

        //현재 씬 이름 확인
        string currentScene = SceneManager.GetActiveScene().name;
        if (currentScene != quest.escortTargetSceneName) return;

        //NPC 위치와 목표 좌표 비교
        float distance = Vector3.Distance(escortNPC.position, quest.escortTargetPosition);
        if (distance <= quest.escortCompleteRadius)
        {
            Debug.Log($"[퀘스트: {questID}] 호위 완료 - NPC가 {quest.escortTargetSceneName}의 목표 위치에 도착");
            QuestManager.Instance.CompleteQuest(quest);
        }
    }
}