using UnityEngine;

// 대화 종료 시 퀘스트 시작 트리거
public class QuestDialogueTrigger : MonoBehaviour, I_DialogueCallback
{
    public string questID;

    public void OnDialogueEnd()
    {
        QuestManager.Instance.StartQuest(questID);
    }
}