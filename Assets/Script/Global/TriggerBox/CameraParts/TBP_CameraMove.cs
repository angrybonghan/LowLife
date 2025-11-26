using UnityEngine;

public class TBP_CameraMove : MonoBehaviour, I_TriggerBox
{
    public enum TriggerBoxPartActionType { atIn, atOut, both }
    [Header("동작 시점")]
    public TriggerBoxPartActionType actionType = TriggerBoxPartActionType.atIn;

    [Header("카메라 위치")]
    public Vector2 cameraPos = Vector2.zero;
    public float cameraZoom = 17;
    public float cameraMoveDuration = 0.5f;

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
        CameraMovement.DollyTo(cameraPos, cameraMoveDuration);
        CameraMovement.PositionZoom(cameraZoom, cameraMoveDuration);
    }
}
