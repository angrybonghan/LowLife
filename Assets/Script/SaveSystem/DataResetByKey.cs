using UnityEngine;
using UnityEngine.SceneManagement;

public class DataResetByKey : MonoBehaviour
{
    void Update()
    {
        // Ctrl + Alt + C 입력 시 전체 데이터 초기화
        if (Input.GetKey(KeyCode.LeftControl) && Input.GetKey(KeyCode.LeftAlt) && Input.GetKeyDown(KeyCode.C))
        {
            Debug.Log("[Ctrl+Alt+C] 전체 데이터 초기화 실행");
            SaveSystemJSON.DataResetByKey("All");
            SaveSystemJSON.ClearItems();

            // QuestManager 메모리 상태 초기화
            var qm = QuestManager.Instance;
            if (qm != null)
            {
                qm.questStates.Clear();
                qm.activeQuests.Clear();

                // allQuests에 있는 퀘스트를 다시 등록 (NotStarted 상태로)
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
        }
    }
}