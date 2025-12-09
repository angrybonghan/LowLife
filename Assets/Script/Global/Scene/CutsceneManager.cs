using UnityEngine;

public class CutsceneManager : MonoBehaviour, I_DialogueCallback
{
    [Header("ÄÆ¾À ID")]
    public string cutsceneID; // ÇöÀç ÄÆ¾ÀÀÇ ID

    [Header("Æ®¸®°Å")]
    public string triggerName = "next";

    [Header("³¡ Scene")]
    public string nextSceneName = "Swomp_1";
    public bool useLoadingScene = true;
    public float fadeOutTime = 1.0f;
    public string loadingSceneName = "StageLoading_1";

    [Header("´ëÈ­")]
    public bool callDialogueOnStart = false;
    public DialogueSO[] dialogueSOs;

    [Header("·¹ÅÍ¹Ú½º")]
    public bool useLetterbox = true;

    [Header("¾÷Àû Å¬¸®¾î")]
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
        // ÄÆ¾À Äù½ºÆ® Å¬¸®¾î ÆÇÁ¤
        QuestManager.Instance?.CompleteCutsceneQuest(cutsceneID);

        // ¾À ÀüÈ¯
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