using UnityEngine;
using System.Collections.Generic;

// 업적 해제를 관리하는 매니저
public class AchievementManager : MonoBehaviour
{
    public static AchievementManager Instance;
    private HashSet<string> unlockedAchievements = new HashSet<string>(); // 해제된 업적 목록

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void UnlockAchievement(string achievementID)
    {
        if (!unlockedAchievements.Contains(achievementID))
        {
            unlockedAchievements.Add(achievementID);
            Debug.Log($"업적 달성: {achievementID}");
            // UI 팝업, 저장 기능 등 확장 가능
        }
    }

    public bool IsUnlocked(string achievementID)
    {
        return unlockedAchievements.Contains(achievementID);
    }
}