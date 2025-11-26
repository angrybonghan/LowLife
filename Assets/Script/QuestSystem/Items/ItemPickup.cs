using UnityEngine;

/// <summary>
/// 아이템 오브젝트에 붙이는 스크립트.
/// 플레이어가 근처에서 F 키로 상호작용하면 아이템을 획득
/// </summary>
public class ItemPickup : MonoBehaviour
{
    public string itemID;       // 아이템 ID
    public string questID;      // 연결된 Collect 퀘스트 ID
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

        if (distance <= interactRange && Input.GetKeyDown(KeyCode.F))
        {
            // 아이템 데이터에 저장
            ItemDatabase.Instance.AddItem(itemID);

            // CollectQuest에 반영
            CollectQuest collectQuest = FindObjectOfType<CollectQuest>();
            if (collectQuest != null)
            {
                collectQuest.AddItem(questID, itemID);
            }

            Debug.Log($"[아이템 획득] {itemID} - 퀘스트 {questID} 진행 반영");

            Destroy(gameObject); // 아이템 오브젝트 제거
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactRange);
    }
}