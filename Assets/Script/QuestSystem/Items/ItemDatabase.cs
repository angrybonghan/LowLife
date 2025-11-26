using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 간단한 아이템 데이터 저장소.
/// 인벤토리 없이 아이템 ID별 개수만 관리.
/// </summary>
public class ItemDatabase : MonoBehaviour
{
    public static ItemDatabase Instance;

    private Dictionary<string, int> itemCounts = new Dictionary<string, int>();

    private void Awake()
    {
        if (Instance == null) Instance = this;
    }

    // 아이템 추가
    public void AddItem(string itemID)
    {
        if (!itemCounts.ContainsKey(itemID))
            itemCounts[itemID] = 0;

        itemCounts[itemID]++;
        Debug.Log($"[아이템 데이터] {itemID} 획득 → 현재 {itemCounts[itemID]}개");
    }

    // 아이템 제거
    public void RemoveItem(string itemID, int amount)
    {
        if (!itemCounts.ContainsKey(itemID)) return;

        itemCounts[itemID] -= amount;
        if (itemCounts[itemID] <= 0)
            itemCounts.Remove(itemID);

        Debug.Log($"[아이템 데이터] {itemID} {amount}개 전달 → 남은 {GetItemCount(itemID)}개");
    }

    // 아이템 개수 조회
    public int GetItemCount(string itemID)
    {
        if (itemCounts.TryGetValue(itemID, out int count))
            return count;
        return 0;
    }
}