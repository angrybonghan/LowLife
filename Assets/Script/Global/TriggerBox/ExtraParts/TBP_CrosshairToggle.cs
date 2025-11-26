using UnityEngine;

public class TBP_CrosshairToggle : MonoBehaviour, I_TriggerBox
{
    public enum TriggerBoxPartActionType { atIn, atOut, both }
    [Header("동작 시점")]
    public TriggerBoxPartActionType actionType = TriggerBoxPartActionType.atIn;

    [Header("조준점")]
    public bool crosshairOn = true;


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
        if (CrosshairController.instance == null) return;

        CrosshairController.instance.ToggleSprite(crosshairOn);
    }
}
