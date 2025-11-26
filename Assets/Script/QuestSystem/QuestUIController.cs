using UnityEngine;
using TMPro;

/// <summary>
/// 퀘스트 UI 컨트롤러.
/// 현재 진행 중인 퀘스트 상태를 TextMeshProUGUI에 표시.
/// </summary>
public class QuestUIController : MonoBehaviour
{
    public TextMeshProUGUI questText; // UI 텍스트 컴포넌트

    private void Update()
    {
        UpdateQuestText();
    }

    // 퀘스트 상태 갱신
    public void UpdateQuestText()
    {
        questText.text = "";

        string currentScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        GameObject playerObj = GameObject.FindWithTag("Player");
        Transform player = playerObj != null ? playerObj.transform : null;

        foreach (var quest in QuestManager.Instance.activeQuests)
        {
            QuestState state = QuestManager.Instance.GetQuestState(quest.questID);
            string line = $"{quest.questName} - {state}";

            // Combat 퀘스트: 남은 적 수 표시
            if (quest.questType == QuestType.Combat)
            {
                int remainingEnemies = Physics2D.OverlapBoxAll(quest.questCenterPosition, new Vector3(quest.detectLeft + quest.detectRight, quest.detectUp + quest.detectDown, 1f), 0f, quest.enemyLayer).Length;

                line += $" (남은 적: {remainingEnemies} 마리)";
            }

            // Explore 퀘스트: 목표 위치까지 남은 거리 표시
            else if (quest.questType == QuestType.Explore && player != null && currentScene == quest.targetSceneName)
            {
                float distance = Vector3.Distance(player.position, quest.exploreTargetPosition);
                line += $" (남은 거리: {distance:F1}m)";
            }

            // Escort 퀘스트: 목표 위치까지 남은 거리 표시
            else if (quest.questType == QuestType.Escort && player != null && currentScene == quest.escortTargetSceneName)
            {
                float distance = Vector3.Distance(player.position, quest.escortTargetPosition);
                line += $" (남은 거리: {distance:F1}m)";
            }

            // Collect 퀘스트: 필요한 아이템 표시
            else if (quest.questType == QuestType.Collect)
            {
                line += $" (필요 아이템: {quest.requiredItemID} x{quest.requiredItemCount})";
            }

            // Delivery 퀘스트: 전달 대상 표시
            else if (quest.questType == QuestType.Delivery)
            {
                line += $" (전달 대상: {quest.deliveryTargetNPC})";
            }

            questText.text += line + "\n";
        }
    }
}