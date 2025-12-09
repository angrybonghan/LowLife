using UnityEngine;

/// <summary>
/// Delivery 퀘스트 처리 스크립트.
/// 플레이어가 NPC와 상호작용(F 키)하면 아이템을 전달하고 퀘스트 완료.
/// </summary>
public class DeliveryQuest : MonoBehaviour
{
    [Header("퀘스트 설정")]
    public string questID;              // 연결된 퀘스트 ID
    public string requiredItemID;       // 전달해야 할 아이템 ID
    public int requiredItemCount = 1;   // 필요한 아이템 개수

    [Header("NPC 설정")]
    public string targetNPCName;        // 전달 대상 NPC 이름
    public GameObject npcObject;        // NPC 오브젝트 (Inspector 연결)
    public bool removeNpcOnComplete = true; // 완료 후 NPC 제거 여부

    [Header("상호작용 설정")]
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
            if (itemCount >= requiredItemCount)
            {
                // 아이템 소모
                ItemDatabase.Instance.RemoveItem(requiredItemID, requiredItemCount);

                // 퀘스트 완료 처리
                QuestManager.Instance.CompleteQuest(quest);
                Debug.Log($"[퀘스트 완료] {quest.questID} - {quest.questName} (NPC {targetNPCName}에게 {requiredItemID} x{requiredItemCount} 전달)");

                UIManager.Instance?.ShowQuestCompleted(quest.questName);
                UIManager.Instance?.UpdateQuestText();

                // NPC 제거 여부 체크
                if (removeNpcOnComplete && npcObject != null)
                {
                    Destroy(npcObject);
                    Debug.Log("[NPC 제거] 아이템 전달 후 NPC가 사라짐");
                }
            }
            else
            {
                Debug.Log($"[퀘스트 실패] {requiredItemID} x{requiredItemCount} 부족 → NPC {targetNPCName}에게 전달 불가");
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, interactRange);
    }
}