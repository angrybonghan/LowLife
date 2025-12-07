using UnityEngine;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Collections.Generic;

public static class SaveSystemJSON
{
    private static string questFilePath = Application.persistentDataPath + "/questData.json";
    private static string stageFilePath = Application.persistentDataPath + "/stageData.json";
    private static string encryptionKey = "MySecretKey12345";

    // 저장 제외할 씬 목록
    private static HashSet<string> excludedScenes = new HashSet<string>
    {
        "MainMenu",
        "PlayerDeathLoading",
        "StageLoading_1",
        "GameStartLoding"
    };

    public static void DataSaveQuests(QuestManager questManager)
    {
        // 전달된 QuestManager가 null이면 저장을 시도하지 않음
        if (questManager == null)
        {
            Debug.LogWarning("QuestManager가 null임");
            return;
        }

        string json = JsonUtility.ToJson(new QuestSaveWrapper(questManager), true);
        string encrypted = Encrypt(json, encryptionKey);
        File.WriteAllText(questFilePath, encrypted);

        string saveTime = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        PlayerPrefs.SetString("LastQuestSaveTime", saveTime);
        PlayerPrefs.Save();
    }

    public static void DataLoadQuests(QuestManager questManager)
    {
        if (!File.Exists(questFilePath)) return;

        string encrypted = File.ReadAllText(questFilePath);
        string decrypted = Decrypt(encrypted, encryptionKey);

        QuestSaveWrapper wrapper = JsonUtility.FromJson<QuestSaveWrapper>(decrypted);
        wrapper.ApplyToManager(questManager);
    }

    // 마지막 스테이지 저장 (저장 제외 씬은 무시)
    public static void DataSaveStage(string stageName)
    {
        if (excludedScenes.Contains(stageName))
        {
            Debug.Log($"[저장 제외] {stageName} 씬은 저장하지 않음");
            return;
        }

        string json = JsonUtility.ToJson(new StageSaveWrapper(stageName), true);
        string encrypted = Encrypt(json, encryptionKey);
        File.WriteAllText(stageFilePath, encrypted);

        Debug.Log($"[스테이지 저장 완료] {stageName}");
    }

    public static string DataLoadStage(string defaultStage = "Swomp_1")
    {
        if (!File.Exists(stageFilePath)) return defaultStage;

        string encrypted = File.ReadAllText(stageFilePath);
        string decrypted = Decrypt(encrypted, encryptionKey);

        StageSaveWrapper wrapper = JsonUtility.FromJson<StageSaveWrapper>(decrypted);
        return wrapper.currentStageName;
    }

    // 클리어한 스테이지 누적 저장 (저장 제외 씬은 무시)
    public static void DataSaveClearedStage(string stageName)
    {
        if (excludedScenes.Contains(stageName))
        {
            Debug.Log($"[저장 제외] {stageName} 씬은 클리어 목록에 추가하지 않음");
            return;
        }

        StageListWrapper wrapper;

        if (File.Exists(stageFilePath))
        {
            string encrypted = File.ReadAllText(stageFilePath);
            string decrypted = Decrypt(encrypted, encryptionKey);
            wrapper = JsonUtility.FromJson<StageListWrapper>(decrypted);
        }
        else
        {
            wrapper = new StageListWrapper();
        }

        if (!wrapper.clearedStages.Contains(stageName))
        {
            wrapper.clearedStages.Add(stageName);
        }

        string json = JsonUtility.ToJson(wrapper, true);
        string encryptedSave = Encrypt(json, encryptionKey);
        File.WriteAllText(stageFilePath, encryptedSave);

        Debug.Log($"[스테이지 클리어 저장] {stageName}");
    }

    public static List<string> DataLoadClearedStages()
    {
        if (!File.Exists(stageFilePath)) return new List<string>();

        string encrypted = File.ReadAllText(stageFilePath);
        string decrypted = Decrypt(encrypted, encryptionKey);

        StageListWrapper wrapper = JsonUtility.FromJson<StageListWrapper>(decrypted);
        return wrapper.clearedStages;
    }

    public static void ClearQuests()
    {
        if (File.Exists(questFilePath)) File.Delete(questFilePath);
    }

    public static void ClearStage()
    {
        if (File.Exists(stageFilePath)) File.Delete(stageFilePath);
    }

    public static void DataResetByKey(string key)
    {
        switch (key)
        {
            case "Quest":
                ClearQuests();
                PlayerPrefs.DeleteKey("LastQuestSaveTime");
                break;
            case "Stage":
                ClearStage();
                break;
            case "All":
                ClearQuests();
                ClearStage();
                PlayerPrefs.DeleteKey("LastQuestSaveTime");
                break;
        }
    }

    private static string Encrypt(string plainText, string key)
    {
        byte[] keyBytes = Encoding.UTF8.GetBytes(key.PadRight(32));
        using (Aes aes = Aes.Create())
        {
            aes.Key = keyBytes;
            aes.IV = new byte[16];
            ICryptoTransform encryptor = aes.CreateEncryptor();
            byte[] bytes = Encoding.UTF8.GetBytes(plainText);
            byte[] encrypted = encryptor.TransformFinalBlock(bytes, 0, bytes.Length);
            return System.Convert.ToBase64String(encrypted);
        }
    }

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

[System.Serializable]
public class StageSaveWrapper
{
    public string currentStageName;
    public StageSaveWrapper(string stageName)
    {
        currentStageName = stageName;
    }
}

[System.Serializable]
public class StageListWrapper
{
    public List<string> clearedStages = new List<string>();
}

[System.Serializable]
public class QuestSaveWrapper
{
    public List<string> activeQuestIDs;
    public List<int> questStates;

    public QuestSaveWrapper(QuestManager manager)
    {
        activeQuestIDs = new List<string>();
        questStates = new List<int>();

        // manager 또는 activeQuests가 null일 경우를 처리
        if (manager == null)
        {
            Debug.LogWarning("QuestSaveWrapper가 null QuestManager임");
            return;
        }
        if (manager.activeQuests == null)
        {
            Debug.LogWarning("QuestManager.activeQuests가 null임");
            return;
        }

        foreach (var quest in manager.activeQuests)
        {
            if (quest == null)
            {
                continue;
            }

            // 퀘스트 ID 추가 (null이면 빈 문자열로 처리)
            string qid = quest.questID ?? string.Empty;
            activeQuestIDs.Add(qid);

            // 퀘스트 상태 가져오기; 문제가 있으면 기본값 0 사용
            if (manager.questStates != null && manager.questStates.ContainsKey(qid))
            {
                questStates.Add((int)manager.questStates[qid]);
            }
            else
            {
                questStates.Add(0);
            }
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