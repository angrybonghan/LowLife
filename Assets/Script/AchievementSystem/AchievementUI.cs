using UnityEngine;

public class AchievementUI : MonoBehaviour
{
    [Header("UI References")]
    public Transform contentParent;            // ScrollView Content
    public GameObject achievementEntryPrefab;  // 업적 항목 프리팹

    private void OnEnable()
    {
        // 창이 열릴 때 업적 목록 갱신
        RefreshAchievements();

        // 업적 달성 이벤트 구독 -> 자동 갱신
        if (AchievementManager.Instance != null)
        {
            AchievementManager.Instance.OnAchievementUnlocked += RefreshAchievements;
        }
    }

    private void OnDisable()
    {
        // 이벤트 구독 해제
        if (AchievementManager.Instance != null)
            AchievementManager.Instance.OnAchievementUnlocked -= RefreshAchievements;
    }

    public void RefreshAchievements()
    {
        // 기존 항목 제거
        foreach (Transform child in contentParent)
        {
            Destroy(child.gameObject);
        }

        if (AchievementManager.Instance != null)
        {
            // 업적 리스트 표시
            foreach (var ach in AchievementManager.Instance.achievements)
            {
                var entry = Instantiate(achievementEntryPrefab, contentParent);
                entry.GetComponent<AchievementEntryUI>().Setup(ach);
            }
        }
    }
}