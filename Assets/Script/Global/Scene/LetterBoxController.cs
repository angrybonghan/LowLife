using UnityEngine;

public class LetterBoxController : MonoBehaviour
{
    public static LetterBoxController Instance { get; private set; }

    [Header("시작")]
    public bool disableAtStart = true;

    [Header("파츠")]
    public LetterBox_Animator[] lb;
    public TMPugui_Animator text;

    [Header("동작의 시간")]
    public float enableTime;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else
        {
            Destroy(gameObject);
            this.enabled = false;
            return;
        }

        if (disableAtStart)
        {
            SetEnableImmediate(false);
        }
    }

    public void ToggleEnable()
    {
        foreach (LetterBox_Animator currentLB in lb)
        {
            currentLB.ToggleBoxEnable(enableTime);
        }
    }

    public void SetEnable(bool isEnable)
    {
        foreach (LetterBox_Animator currentLB in lb)
        {
            currentLB.SetLetterBoxEnable(isEnable, enableTime);
        }
    }

    public void SetEnableImmediate(bool isEnable)
    {
        foreach (LetterBox_Animator currentLB in lb)
        {
            currentLB.SetLetterBoxEnable(isEnable, 0);
        }
    }

    public void SetText(string Text)
    {
        text.SetText(Text);
    }
}
