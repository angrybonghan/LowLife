using UnityEngine;
using System.Collections;

public class DialogManager : MonoBehaviour
{
    public static DialogManager instance;

    public DialogueBubble dialogueBubblePrefab;

    [Header("소리")]
    public AudioClip dialogueTypingSound;

    bool isTyping;         // 현재 타이핑 중인지 여부
    bool skipInputDetected = false; // 스킵 키가 입력되었는지 여부

    GameObject dialogueCallbackObj;
    DialogueBubble currentBubbleInstance;
    Vector3 currentBubblePos;

    DialogueSO currentDialogue;
    Coroutine dialogueCoroutine;

    void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);
    }

    void Update()
    {
        if (IsAnySkipKeyPressed() && isTyping)
        {
            skipInputDetected = true;
        }
    }

    public void StartDialogue(DialogueSO dialogue, GameObject CallbackObj)
    {
        if (dialogue == null) return;

        dialogueCallbackObj = CallbackObj;

        if (dialogueCoroutine != null)
        {
            StopCoroutine(dialogueCoroutine);
            dialogueCoroutine = null;
        }
        if (currentBubbleInstance != null)
        {
            Destroy(currentBubbleInstance.gameObject);
            currentBubbleInstance = null;
        }

        if (dialogueBubblePrefab != null)
        {
            currentBubbleInstance = Instantiate(dialogueBubblePrefab, Vector3.one * 10191019, Quaternion.identity);
        }
        else
        {
            Debug.LogError("DialogueBubble 프리팹 없음");
            return;
        }

        currentDialogue = dialogue;
        dialogueCoroutine = StartCoroutine(PlayDialogueSequence());
    }

    public void StopDialogue()
    {
        if (dialogueCoroutine != null)
        {
            StopCoroutine(dialogueCoroutine);
            dialogueCoroutine = null;
        }

        if (currentBubbleInstance != null)
        {
            Destroy(currentBubbleInstance.gameObject);
            currentBubbleInstance = null;
        }

        isTyping = false;
    }

    IEnumerator PlayDialogueSequence()
    {
        if (currentDialogue.section == null)
        {
            StopDialogue();
            yield break;
        }

        yield return null;

        foreach (DialogueLine line in currentDialogue.section)
        {
            UpdateBubbleUI(line); // 말풍선 위치, 꼬리표 방향 등 UI 업데이트

            if (line.sentence == null || line.sentence.Length == 0) continue;

            foreach (string sentence in line.sentence)
            {
                if (string.IsNullOrWhiteSpace(sentence)) continue;

                yield return StartCoroutine(TypeSentence(sentence, line.intervalTime, line.canSkip));

                if (line.canSkip && line.autoSkip)
                {
                    yield return StartCoroutine(WaitForInputOrTime(line.autoSkipIntervalTime));
                }
                else if (line.canSkip && !line.autoSkip)
                {
                    yield return StartCoroutine(WaitForAnyKeyInput());
                }
                else if (!line.canSkip && line.autoSkip)
                {
                    yield return new WaitForSeconds(line.autoSkipIntervalTime);
                }
                else
                {
                    yield return StartCoroutine(WaitForAnyKeyInput());
                }
            }
        }

        I_DialogueCallback[] callbacks = dialogueCallbackObj.GetComponents<I_DialogueCallback>();

        foreach (I_DialogueCallback callback in callbacks)
        {
            callback.OnDialogueEnd();
        }

        StopDialogue();
    }

    IEnumerator TypeSentence(string fullSentence, float intervalTime, bool canSkipTyping)
    {
        skipInputDetected = false;
        isTyping = true;
        string currentText = "";
        currentBubbleInstance.SetText("");
        yield return null;

        for (int i = 0; i < fullSentence.Length; i++)
        {
            if (canSkipTyping && skipInputDetected)
            {
                currentBubbleInstance.SetText(fullSentence);
                currentText = fullSentence;
                isTyping = false;
                skipInputDetected = false;
                break;
            }

            currentText += fullSentence[i];
            currentBubbleInstance.SetText(currentText);

            AudioManager.Instance.Play3DSound(dialogueTypingSound, currentBubblePos);

            yield return new WaitForSeconds(intervalTime);
        }

        if (currentText.Length < fullSentence.Length)
        {
            currentBubbleInstance.SetText(fullSentence);
        }

        isTyping = false;
        skipInputDetected = false;
    }

    private void UpdateBubbleUI(DialogueLine line)
    {
        if (line.bubblePosition != Vector3.zero)
        {
            currentBubbleInstance.SetPosition(line.bubblePosition);
            currentBubblePos = line.bubblePosition;
        }
        
        currentBubbleInstance.SetBubbleOffset(line.bubbleBodyOffset);
        currentBubbleInstance.SetTailToLower(line.isTailDown);
    }

    IEnumerator WaitForAnyKeyInput()
    {
        yield return null;
        while (IsAnySkipKeyPressed())
        {
            yield return null;
        }

        while (!IsAnySkipKeyPressed())
        {
            yield return null;
        }
    }

    IEnumerator WaitForInputOrTime(float duration)
    {
        yield return null;
        while (IsAnySkipKeyPressed())
        {
            yield return null;
        }

        float startTime = Time.time;

        while (Time.time < startTime + duration && !IsAnySkipKeyPressed())
        {
            yield return null;
        }
    }

    bool IsAnySkipKeyPressed()
    {
        bool isKeyboardPressedDown = Input.GetKeyDown(KeyCode.Space) ||
                                     Input.GetKeyDown(KeyCode.F) ||
                                     Input.GetKeyDown(KeyCode.E);

        bool isMousePressedDown = Input.GetMouseButtonDown(0) ||
                                  Input.GetMouseButtonDown(1) ||
                                  Input.GetMouseButtonDown(2);

        return isKeyboardPressedDown || isMousePressedDown;
    }

}
