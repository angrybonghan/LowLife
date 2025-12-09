using UnityEngine;

/// <summary>
/// 아이템 오브젝트에 붙이는 스크립트.
/// 플레이어가 근처에서 F 키로 상호작용하면 아이템을 획득하고,
/// ItemDatabase에 저장한 뒤 CollectQuest에 반영.
/// </summary>
public class ItemPickup : MonoBehaviour
{
    [Header("아이템 설정")]
    public string itemID;        // 아이템 ID
    public int itemCount = 1;    // 획득 개수
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
            // 업적 반영
            AchievementManager.Instance?.OnItemCollected(itemID, itemCount);

            // 아이템 데이터베이스에 저장 (SaveSystemJSON을 통해 영구 저장됨)
            ItemDatabase.Instance.AddItem(itemID, itemCount);

            // 수집 퀘스트 반영
            CollectQuest collectQuest = FindObjectOfType<CollectQuest>();
            if (collectQuest != null)
            {
                collectQuest.CheckAllCollectQuests(itemID);
            }

            Debug.Log($"[아이템 획득] {itemID} x{itemCount} → CollectQuest 반영 및 저장 완료");

            Destroy(gameObject); // 아이템 오브젝트 제거
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactRange);
    }
}