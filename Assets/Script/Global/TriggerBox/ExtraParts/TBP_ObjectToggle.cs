using UnityEngine;

public class TBP_ObjectToggle : MonoBehaviour, I_TriggerBox
{
    public enum TriggerBoxPartActionType { atIn, atOut, both }
    [Header("동작 시점")]
    public TriggerBoxPartActionType actionType = TriggerBoxPartActionType.atIn;

    [Header("시작 시 목표 반전")]
    public bool triggerAtStart = false;
    // Start() 에서 켜질 오브젝트를 끄고, 꺼질 오브젝트는 키는 동작

    [Header("켜질 오브젝트")]
    public GameObject[] onTarget;
    [Header("꺼질 오브젝트")]
    public GameObject[] offTarget;

    private void Start()
    {
        if (triggerAtStart)
        {
            foreach (GameObject obj in onTarget)
            {
                obj.SetActive(false);
            }

            foreach (GameObject obj in offTarget)
            {
                obj.SetActive(true);
            }
        }
    }

    public void TriggerIn()
    {
        if (actionType == TriggerBoxPartActionType.atOut) return;

        Trigger();
    }

    public void TriggerOut()
    {
        if (actionType == TriggerBoxPartActionType.atIn) return;

        Trigger();
    }

    void Trigger()
    {
        foreach (GameObject obj in onTarget)
        {
            obj.SetActive(true);
        }

        foreach (GameObject obj in offTarget)
        {
            obj.SetActive(false);
        }
    }
}
