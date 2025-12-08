using UnityEngine;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Collections.Generic;

public static class SaveSystemJSON
{
    private static string questFilePath = Application.persistentDataPath + "/questData.json";
    private static string stageFilePath = Application.persistentDataPath + "/stageData.json";
    private static string achievementFilePath = Application.persistentDataPath + "/achievementData.json"; //업적 저장 파일
    private static string encryptionKey = "MySecretKey12345";

    // 저장 제외할 씬 목록
    private static HashSet<string> excludedScenes = new HashSet<string>
    {
        "MainMenu",
        "PlayerDeathLoading",
        "StageLoading_1",
        "GameStartLoding"
    };

    //퀘스트 저장/로드
    public static void DataSaveQuests(QuestManager questManager)
    {
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

    //스테이지 저장/로드
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

    // 업적 저장/로드
    public static void DataSaveAchievements(AchievementManager achievementManager)
    {
        if (achievementManager == null)
        {
            Debug.LogWarning("AchievementManager가 null임");
            return;
        }

        string json = JsonUtility.ToJson(new AchievementSaveWrapper(achievementManager), true);
        string encrypted = Encrypt(json, encryptionKey);
        File.WriteAllText(achievementFilePath, encrypted);

        Debug.Log("[업적 저장 완료]");
    }

    public static void DataLoadAchievements(AchievementManager achievementManager)
    {
        if (!File.Exists(achievementFilePath)) return;

        string encrypted = File.ReadAllText(achievementFilePath);
        string decrypted = Decrypt(encrypted, encryptionKey);

        AchievementSaveWrapper wrapper = JsonUtility.FromJson<AchievementSaveWrapper>(decrypted);
        wrapper.ApplyToManager(achievementManager);

        Debug.Log("[업적 로드 완료]");
    }

    // 데이터 초기화
    public static void ClearQuests()
    {
        if (File.Exists(questFilePath)) File.Delete(questFilePath);
    }

    public static void ClearStage()
    {
        if (File.Exists(stageFilePath)) File.Delete(stageFilePath);
    }

    public static void ClearAchievements()
    {
        if (File.Exists(achievementFilePath)) File.Delete(achievementFilePath);
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
            case "Achievement":
                ClearAchievements();
                break;
            case "All":
                ClearQuests();
                ClearStage();
                ClearAchievements();
                PlayerPrefs.DeleteKey("LastQuestSaveTime");
                break;
        }
    }

    // 암호화/복호화
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

//Wrapper Classes
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

        if (manager == null || manager.activeQuests == null) return;

        foreach (var quest in manager.activeQuests)
        {
            if (quest == null) continue;

            string qid = quest.questID ?? string.Empty;
            activeQuestIDs.Add(qid);

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

[System.Serializable]
public class AchievementSaveWrapper
{
    public List<string> achievementIDs = new List<string>();
    public List<bool> unlockedStates = new List<bool>();
    public List<int> currentCounts = new List<int>();

    public AchievementSaveWrapper(AchievementManager manager)
    {
        foreach (var ach in manager.achievements)
        {
            achievementIDs.Add(ach.achievementID);
            unlockedStates.Add(ach.isUnlocked);
            currentCounts.Add(ach.currentCount);
        }
    }

    public void ApplyToManager(AchievementManager manager)
    {
        for (int i = 0; i < achievementIDs.Count; i++)
        {
            var ach = manager.achievements.Find(a => a.achievementID == achievementIDs[i]);
            if (ach != null)
            {
                ach.isUnlocked = unlockedStates[i];
                ach.currentCount = currentCounts[i];
            }
        }
    }
}