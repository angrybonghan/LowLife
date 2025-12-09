using UnityEngine;
using System.Collections.Generic;

public class ItemDatabase : MonoBehaviour
{
    public static ItemDatabase Instance;

    // 아이템 상태 저장용 Dictionary
    public Dictionary<string, int> items = new Dictionary<string, int>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadItems(); // 게임 시작 시 저장된 아이템 불러오기
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void AddItem(string itemID, int count)
    {
        if (items.ContainsKey(itemID))
            items[itemID] += count;
        else
            items[itemID] = count;

        SaveItems();
    }

    public void RemoveItem(string itemID, int count)
    {
        if (items.ContainsKey(itemID))
        {
            items[itemID] -= count;
            if (items[itemID] <= 0) items.Remove(itemID);
            SaveItems();
        }
    }

    public int GetItemCount(string itemID)
    {
        return items.ContainsKey(itemID) ? items[itemID] : 0;
    }

    // SaveSystemJSON 연동
    public void SaveItems()
    {
        SaveSystemJSON.DataSaveItems(this);
    }

    public void LoadItems()
    {
        SaveSystemJSON.DataLoadItems(this);
    }
}