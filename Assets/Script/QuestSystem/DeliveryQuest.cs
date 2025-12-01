using UnityEngine;

/// <summary>
/// Delivery 퀘스트 처리 스크립트.
/// 플레이어가 NPC와 상호작용(F 키)하면 아이템을 전달하고 퀘스트 완료.
/// </summary>
public class DeliveryQuest : MonoBehaviour
{
    public string questID;          // 연결된 퀘스트 ID
    public string requiredItemID;   // 전달해야 할 아이템 ID
    public string targetNPCName;    // 전달 대상 NPC 이름
    public float interactRange = 2f;

    private Transform player;

    private void Start()
    {
        GameObject playerObj = GameObject.FindWithTag("Player");
        if (playerObj != null) player = playerObj.transform;
    }

    private void Update()
    {
        if (player == null) return;

        float distance = Vector3.Distance(player.position, transform.position);

        // 플레이어가 NPC 근처에서 F 키를 누르면 전달 시도
        if (distance <= interactRange && Input.GetKeyDown(KeyCode.F))
        {
            QuestDataSO quest = QuestManager.Instance.GetQuestData(questID);
            if (quest == null || quest.questType != QuestType.Delivery) return;
            if (QuestManager.Instance.GetQuestState(questID) != QuestState.InProgress) return;

            // ItemDatabase에서 아이템 보유 여부 확인
            int itemCount = ItemDatabase.Instance.GetItemCount(requiredItemID);
            if (itemCount > 0)
            {
                // 아이템 하나 소모
                ItemDatabase.Instance.RemoveItem(requiredItemID, 1);

                // 퀘스트 완료 처리
                QuestManager.Instance.CompleteQuest(quest);
                Debug.Log($"[퀘스트 완료] {quest.questID} - {quest.questName} (NPC {targetNPCName}에게 {requiredItemID} 전달)");

                FindObjectOfType<UIManager>()?.UpdateQuestText();
            }
            else
            {
                Debug.Log($"[퀘스트 실패] {requiredItemID}가 없어 NPC {targetNPCName}에게 전달 불가");
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, interactRange);
    }
}