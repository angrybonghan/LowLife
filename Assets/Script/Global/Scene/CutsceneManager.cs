using UnityEngine;

public class CutsceneManager : MonoBehaviour, I_DialogueCallback
{
    [Header("컷씬 ID")]
    public string cutsceneID; // 현재 컷씬의 ID (QuestDataSO.cutsceneID와 매칭)

    [Header("트리거")]
    public string triggerName = "next";

    [Header("끝 Scene")]
    public string nextSceneName = "Swomp_1";
    public bool useLoadingScene = true;
    public float fadeOutTime = 1.0f;
    public string loadingSceneName = "StageLoading_1";

    [Header("대화")]
    public bool callDialogueOnStart = false;
    public DialogueSO[] dialogueSOs;

    [Header("레터박스")]
    public bool useLetterbox = true;

    [Header("업적 클리어")]
    public bool isClearAchievement;
    public string achievementID;

    int currentDialogueIndex = 0;
    Animator anim;

    private void Awake()
    {
        anim = GetComponent<Animator>();
    }

    void Start()
    {
        if (callDialogueOnStart)
            CallDialogue();

        if (useLetterbox)
            LetterBoxController.Instance.SetEnable(true);

        if (isClearAchievement)
            AchievementManager.Instance?.OnTalkToNPC(achievementID);
    }

    public void CallDialogue()
    {
        if (dialogueSOs.Length > currentDialogueIndex)
        {
            DialogManager.instance.StartDialogue(dialogueSOs[currentDialogueIndex], gameObject);
            currentDialogueIndex++;
        }
    }

    public void OnDialogueEnd()
    {
        anim.SetTrigger(triggerName);
    }

    public void LoadNextScene()
    {
        if (QuestManager.Instance != null)
        {
            // 컷씬 퀘스트 처리
            foreach (var quest in QuestManager.Instance.activeQuests)
            {
                if (quest.questType == QuestType.Cutscene &&
                    quest.cutsceneID == cutsceneID)
                {
                    var state = QuestManager.Instance.GetQuestState(quest.questID);

                    // 선행 퀘스트가 없는 경우 -> 바로 시작하고 즉시 클리어
                    if (string.IsNullOrEmpty(quest.prerequisiteQuestID) && state == QuestState.NotStarted)
                    {
                        QuestManager.Instance.StartQuest(quest.questID);
                        QuestManager.Instance.CompleteQuest(quest);
                        Debug.Log($"[CutsceneManager] 선행 없음 -> {quest.questName} 즉시 시작 및 클리어");
                    }
                    // 이미 진행 중인 경우 -> 클리어 처리
                    else if (state == QuestState.InProgress)
                    {
                        QuestManager.Instance.CompleteQuest(quest);
                        Debug.Log($"[CutsceneManager] 컷씬({cutsceneID}) 종료 -> 퀘스트 클리어: {quest.questName}");
                    }

                    break;
                }
            }
        }
        

        // 씬 전환
        ScreenTransition.ScreenTransitionGoto(
            nextSceneName,
            loadingSceneName,
            Color.black,
            0f,
            fadeOutTime,
            useLoadingScene ? 2.0f : 0,
            0.7887f,
            0f
        );
    }
}