using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Explore 퀘스트 처리 스크립트.
/// 플레이어가 특정 씬의 특정 위치에 도달하면 퀘스트 완료.
/// 여러 Explore 퀘스트를 동시에 처리 가능.
/// </summary>
public class ExploreQuest : MonoBehaviour
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
            if (quest.questType != QuestType.Explore) continue;
            if (QuestManager.Instance.GetQuestState(quest.questID) != QuestState.InProgress) continue;
            if (currentScene != quest.targetSceneName) continue;

            float distance = Vector3.Distance(player.position, quest.exploreTargetPosition);
            if (distance <= quest.exploreRadius)
            {
                Debug.Log($"[Explore 완료] {quest.questID} - {quest.questName}");
                UIManager.Instance?.ShowQuestCompleted(quest.questName);
                QuestManager.Instance.CompleteQuest(quest);
            }
        }
    }
}