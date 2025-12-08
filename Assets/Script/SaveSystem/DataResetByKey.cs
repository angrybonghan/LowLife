using UnityEngine;
using UnityEngine.SceneManagement;

public class DataResetByKey : MonoBehaviour
{
    void Update()
    {
        if (Input.GetKey(KeyCode.LeftControl) && Input.GetKey(KeyCode.LeftAlt) && Input.GetKeyDown(KeyCode.C))
        {
            Debug.Log("[키 입력 감지: Ctrl+Alt+C] 퀘스트 + 스테이지 + 업적 데이터 전체 초기화 실행");

            // 저장 데이터 초기화 (퀘스트, 스테이지, 업적 모두)
            SaveSystemJSON.DataResetByKey("All");

            // QuestManager 메모리 상태도 초기화
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

            // AchievementManager 메모리 상태도 초기화
            var am = AchievementManager.Instance;
            if (am != null)
            {
                foreach (var ach in am.achievements)
                {
                    ach.isUnlocked = false;
                    ach.currentCount = 0;
                }
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