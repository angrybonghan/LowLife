using UnityEngine;
using TMPro;
using UnityEngine.UI;

/// <summary>
/// 업적 항목 UI를 담당하는 스크립트
/// - 업적 이름, 설명/힌트, 아이콘, 자물쇠 이미지를 표시
/// - 업적 달성 시 자물쇠 이미지를 숨김
/// </summary>
public class AchievementEntryUI : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI titleText;       // 업적 이름 표시
    public TextMeshProUGUI descriptionText; // 업적 설명 또는 힌트 표시
    public Image statusIcon;                // 달성 여부 아이콘
    public GameObject lockImage;            // 자물쇠 이미지 (달성 전 표시)

    private AchievementDataSO achievement;  // 연결된 업적 데이터

    public void Setup(AchievementDataSO ach)
    {
        achievement = ach;
        UpdateUI();
    }

    public void UpdateUI()
    {
        if (achievement == null) return;

        // 업적 이름
        titleText.text = achievement.title;

        // 달성 여부에 따라 설명/힌트 표시
        if (achievement.isUnlocked)
        {
            descriptionText.text = $"달성됨 - {achievement.description}";
            if (lockImage != null) lockImage.SetActive(false);

            // 아이콘 교체 (달성 아이콘만 사용)
            if (statusIcon != null && achievement.unlockedIcon != null)
                statusIcon.sprite = achievement.unlockedIcon;
        }
        else
        {
            descriptionText.text = $"힌트: {achievement.hint}";
            if (lockImage != null) lockImage.SetActive(true);

            // 달성 전에는 아이콘을 기본 회색 처리
            if (statusIcon != null)
                statusIcon.color = Color.gray;
        }
    }
}