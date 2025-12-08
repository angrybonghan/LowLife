using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// 플레이어 위치를 체크해서 Explore / Escort 퀘스트를 완료 처리하는 스크립트.
/// 여러 개의 이동 퀘스트를 동시에 지원.
/// </summary>
public class PlayerPositionChecker : MonoBehaviour
{
    private Transform player;

    private void Start()
    {
        // Player 태그가 붙은 오브젝트 자동 참조
        GameObject playerObj = GameObject.FindWithTag("Player");
        if (playerObj != null) player = playerObj.transform;
    }

    private void Update()
    {
        if (player == null) return;

        string currentScene = SceneManager.GetActiveScene().name;

        foreach (var quest in QuestManager.Instance.activeQuests)
        {
            // 진행 중인 퀘스트만 체크
            if (QuestManager.Instance.GetQuestState(quest.questID) != QuestState.InProgress) continue;

            // Explore 퀘스트 처리
            if (quest.questType == QuestType.Explore && currentScene == quest.targetSceneName)
            {
                float distance = Vector3.Distance(player.position, quest.exploreTargetPosition);
                if (distance <= quest.exploreRadius)
                {
                    Debug.Log($"[Explore 완료] {quest.questID} - {quest.questName}");
                    UIManager.Instance?.ShowQuestCompleted(quest.questName);
                    QuestManager.Instance.CompleteQuest(quest);
                }
            }

            // Escort 퀘스트 처리
            if (quest.questType == QuestType.Escort && currentScene == quest.escortTargetSceneName)
            {
                float distance = Vector3.Distance(player.position, quest.escortTargetPosition);
                if (distance <= quest.escortCompleteRadius)
                {
                    Debug.Log($"[Escort 완료] {quest.questID} - {quest.questName}");
                    UIManager.Instance?.ShowQuestCompleted(quest.questName);
                    QuestManager.Instance.CompleteQuest(quest);
                }
            }
        }
    }
}