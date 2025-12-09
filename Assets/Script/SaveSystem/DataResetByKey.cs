using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class DataResetByKey : MonoBehaviour
{
    public TextMeshProUGUI CleerText;

    private void Start()
    {
        CleerText.gameObject.SetActive(false);
    }

    public void ResetCode()
    {
        Debug.Log("[Ctrl+Alt+C] 전체 데이터 초기화 실행");
        SaveSystemJSON.DataResetByKey("All");
        SaveSystemJSON.ClearItems();

        PlayerPrefs.DeleteAll();   // 모든 PlayerPrefs 데이터 삭제
        PlayerPrefs.Save();        // 즉시 반영
        Debug.Log("[저장 초기화] 모든 게임 데이터가 초기화되었습니다.");

        // QuestManager 메모리 상태 초기화
        var qm = QuestManager.Instance;
        if (qm != null)
        {
            qm.questStates.Clear();
            qm.activeQuests.Clear();

            foreach (var quest in qm.allQuests)
            {
                qm.AddToActiveQuests(quest);
            }
        }

        // AchievementManager 메모리 상태 초기화
        var am = AchievementManager.Instance;
        if (am != null)
        {
            foreach (var ach in am.achievements)
            {
                ach.isUnlocked = false;
                ach.currentCount = 0;
            }
        }

        // ItemDatabase 메모리 상태 초기화
        var itemDB = ItemDatabase.Instance;
        if (itemDB != null)
        {
            itemDB.items.Clear();
        }

        // UI 반영
        var uiManager = FindObjectOfType<UIManager>();
        if (uiManager != null)
        {
            uiManager.UpdateQuestText();
            uiManager.UpdateSaveTimeText("초기화됨");
        }

        // 잠깐 표시할 텍스트 실행
        if (CleerText != null)
        {
            StartCoroutine(ShowResetText());
        }
    }

    private IEnumerator ShowResetText()
    {
        CleerText.text = "데이터가 초기화되었습니다!";
        CleerText.gameObject.SetActive(true);

        yield return new WaitForSeconds(2f); // 2초 동안 표시

        CleerText.gameObject.SetActive(false);
    }
}