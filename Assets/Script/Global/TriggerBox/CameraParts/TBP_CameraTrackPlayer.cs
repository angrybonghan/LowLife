using UnityEngine;

public class TBP_CameraTrackPlayer : MonoBehaviour, I_TriggerBox
{
    public enum TriggerBoxPartActionType { atIn, atOut, both }
    [Header("동작 시점")]
    public TriggerBoxPartActionType actionType = TriggerBoxPartActionType.atIn;

    [Header("카메라 Zoom")]
    public float cameraZoom = 17;
    public float cameraMoveDuration = 0.5f;

    Transform playerTransform;

    private void Start()
    {
        playerTransform = PlayerController.instance.transform;
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
        CameraMovement.PositionZoom(cameraZoom, cameraMoveDuration);
        CameraMovement.TargetTracking(playerTransform, Vector3.up);
    }
}
