using UnityEngine;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Collections.Generic;

/// <summary>
/// JSON + AES 암호화 기반 퀘스트 저장 시스템.
/// QuestManager와 연동해서 퀘스트 상태를 파일로 저장/불러오기/초기화.
/// </summary>
public static class QuestSaveSystemJSON
{
    private static string filePath = Application.persistentDataPath + "/questData.json";
    private static string encryptionKey = "MySecretKey12345"; //암호화 키 (프로젝트 내에서 안전하게 관리)

    /// <summary>
    /// 퀘스트 저장
    /// </summary>
    public static void SaveQuests(QuestManager questManager)
    {
        string json = JsonUtility.ToJson(new QuestSaveWrapper(questManager), true);

        // 암호화
        string encrypted = Encrypt(json, encryptionKey);

        File.WriteAllText(filePath, encrypted);
        Debug.Log("[퀘스트 저장 완료 - JSON 암호화]");
    }

    /// <summary>
    /// 퀘스트 불러오기
    /// </summary>
    public static void LoadQuests(QuestManager questManager)
    {
        if (!File.Exists(filePath)) return;

        string encrypted = File.ReadAllText(filePath);
        string decrypted = Decrypt(encrypted, encryptionKey);

        QuestSaveWrapper wrapper = JsonUtility.FromJson<QuestSaveWrapper>(decrypted);
        wrapper.ApplyToManager(questManager);

        Debug.Log("[퀘스트 불러오기 완료 - JSON 복호화]");
    }

    /// <summary>
    /// 퀘스트 데이터 초기화 (저장 파일 삭제)
    /// </summary>
    public static void ClearQuests()
    {
        if (File.Exists(filePath))
        {
            File.Delete(filePath);
            Debug.Log("[퀘스트 저장 데이터 초기화]");
        }
    }

    // AES 암호화
    private static string Encrypt(string plainText, string key)
    {
        byte[] keyBytes = Encoding.UTF8.GetBytes(key.PadRight(32));
        using (Aes aes = Aes.Create())
        {
            aes.Key = keyBytes;
            aes.IV = new byte[16]; // 초기화 벡터 (간단히 0으로)
            ICryptoTransform encryptor = aes.CreateEncryptor();
            byte[] bytes = Encoding.UTF8.GetBytes(plainText);
            byte[] encrypted = encryptor.TransformFinalBlock(bytes, 0, bytes.Length);
            return System.Convert.ToBase64String(encrypted);
        }
    }

    // AES 복호화
    private static string Decrypt(string cipherText, string key)
    {
        byte[] keyBytes = Encoding.UTF8.GetBytes(key.PadRight(32));
        using (Aes aes = Aes.Create())
        {
            aes.Key = keyBytes;
            aes.IV = new byte[16];
            ICryptoTransform decryptor = aes.CreateDecryptor();
            byte[] bytes = System.Convert.FromBase64String(cipherText);
            byte[] decrypted = decryptor.TransformFinalBlock(bytes, 0, bytes.Length);
            return Encoding.UTF8.GetString(decrypted);
        }
    }
}

/// <summary>
/// 퀘스트 저장용 래퍼 클래스.
/// QuestManager의 activeQuests와 questStates를 직렬화/역직렬화.
/// </summary>
[System.Serializable]
public class QuestSaveWrapper
{
    public List<string> activeQuestIDs;
    public List<int> questStates;

    public QuestSaveWrapper(QuestManager manager)
    {
        activeQuestIDs = new List<string>();
        questStates = new List<int>();

        foreach (var quest in manager.activeQuests)
        {
            activeQuestIDs.Add(quest.questID);
            questStates.Add((int)manager.questStates[quest.questID]);
        }
    }

    public void ApplyToManager(QuestManager manager)
    {
        for (int i = 0; i < activeQuestIDs.Count; i++)
        {
            QuestDataSO quest = manager.GetQuestData(activeQuestIDs[i]);
            if (quest != null)
            {
                manager.AddToActiveQuests(quest);
                manager.ForceSetQuestState(activeQuestIDs[i], (QuestState)questStates[i]);
            }
        }
    }
}