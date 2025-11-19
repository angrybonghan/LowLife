using UnityEngine;

// NPC 대화 종료 후 퀘스트 시작을 트리거하는 컴포넌트
public class QuestDialogueTrigger : MonoBehaviour, I_DialogueCallback
{
    public string questID;

    public void OnDialogueEnd()
    {
        QuestManager.Instance.StartQuest(questID);
    }
}