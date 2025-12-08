using UnityEngine;

public class CutsceneManager : MonoBehaviour, I_DialogueCallback
{
    [Header("트리거")]
    public string triggerName = "next";

    [Header("끝 Scene")]
    public string nextSceneName = "Swomp_1";
    public string loadingSceneName = "StageLoading_1";

    [Header("대화")]
    public bool callDialogueOnStart = false;
    public DialogueSO[] dialogueSOs;

    [Header("레터박스")]
    public bool useLetterbox = true;


    int currentDialogueIndex = 0;

    Animator anim;

    private void Awake()
    {
        anim = GetComponent<Animator>();
    }

    void Start()
    {
        if (callDialogueOnStart)
        {
            CallDialogue();
        }

        if (useLetterbox)
        {
            LetterBoxController.Instance.SetEnable(true);
        }
    }

    public void CallDialogue()
    {
        DialogManager.instance.StartDialogue(dialogueSOs[currentDialogueIndex], gameObject);
        currentDialogueIndex++;
    }

    public void OnDialogueEnd()
    {
        anim.SetTrigger(triggerName);
    }

    public void LoadNextScene()
    {
        ScreenTransition.ScreenTransitionGoto(
            nextSceneName,
            loadingSceneName,
            Color.black,
            0f,
            1.0f,
            2.0f,
            1.0f,
            0f
        );
    }
}
