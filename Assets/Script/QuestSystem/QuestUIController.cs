using UnityEngine;
using TMPro;

public class QuestUIController : MonoBehaviour
{
    public TextMeshProUGUI questText;

    private void Start()
    {
        UpdateQuestText();
    }

    public void UpdateQuestText()
    {
        string display = "";

        foreach (var quest in QuestManager.Instance.GetActiveQuests())
        {
            var state = QuestManager.Instance.GetQuestState(quest.questID);

            // 완료된 퀘스트는 UI에서 숨김
            if (state == QuestState.Completed)
                continue;

            // 선행 퀘스트가 있고, 아직 완료되지 않았다면 숨김
            if (!string.IsNullOrEmpty(quest.prerequisiteQuestID))
            {
                var prereqState = QuestManager.Instance.GetQuestState(quest.prerequisiteQuestID);
                if (prereqState != QuestState.Completed)
                    continue;
            }

            // 표시할 퀘스트 정보
            display += $"<b>{quest.questName}</b>\n{quest.description}\n\n";
        }

        questText.text = display;
    }
}