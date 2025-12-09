using UnityEngine;
using System.Collections;

public class DeliveryQuest : MonoBehaviour
{
    public string questID;
    public string requiredItemID;
    public int requiredItemCount = 1;

    public GameObject npcObject;
    public bool removeNpcOnComplete = true;

    public float interactRange = 2f;

    // 이동 설정 (Vector2 사용)
    public Vector2 startPoint;
    public Vector2 endPoint;
    public float moveDuration = 1.5f;

    private Transform player;

    private void Start()
    {
        GameObject playerObj = GameObject.FindWithTag("Player");
        if (playerObj != null) player = playerObj.transform;

        // 시작 위치 적용
        if (npcObject != null)
        {
            npcObject.transform.position = new Vector3(startPoint.x, startPoint.y, npcObject.transform.position.z);
        }
    }

    private void Update()
    {
        if (player == null) return;

        float distance = Vector3.Distance(player.position, transform.position);

        if (distance <= interactRange && Input.GetKeyDown(KeyCode.F))
        {
            QuestDataSO quest = QuestManager.Instance.GetQuestData(questID);
            if (quest == null || quest.questType != QuestType.Delivery) return;
            if (QuestManager.Instance.GetQuestState(questID) != QuestState.InProgress) return;

            int itemCount = ItemDatabase.Instance.GetItemCount(requiredItemID);
            if (itemCount >= requiredItemCount)
            {
                ItemDatabase.Instance.RemoveItem(requiredItemID, requiredItemCount);
                QuestManager.Instance.CompleteQuest(quest);

                Debug.Log($"[퀘스트 완료] {quest.questID} - {quest.questName}");

                if (removeNpcOnComplete && npcObject != null)
                {
                    StartCoroutine(MoveNpcAndRemove(npcObject));
                }
            }
        }
    }

    private IEnumerator MoveNpcAndRemove(GameObject npc)
    {
        float elapsed = 0f;
        Vector2 startPos = startPoint;
        Vector2 targetPos = endPoint;

        while (elapsed < moveDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / moveDuration;

            // Vector2로 보간 후 Vector3로 변환
            Vector2 newPos = Vector2.Lerp(startPos, targetPos, t);
            npc.transform.position = new Vector3(newPos.x, newPos.y, npc.transform.position.z);

            yield return null;
        }

        Destroy(npc);
        Debug.Log("[NPC 제거] Vector2 기반 이동 후 제거됨");
    }
}