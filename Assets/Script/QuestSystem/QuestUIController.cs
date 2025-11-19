using UnityEngine;
using TMPro;

// 퀘스트 UI 텍스트 표시
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
            display += $"<b>{quest.questName}</b> - {state}\n{quest.description}\n\n";
        }

        questText.text = display;
    }
}