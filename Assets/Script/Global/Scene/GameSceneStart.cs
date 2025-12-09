using System.Collections;
using UnityEngine;

public class GameSceneStart : MonoBehaviour
{
    [Header("이동")]
    public bool toThisObject = false;
    public float TargetPosX = 0;

    [Header("조준점")]
    public bool hideCrosshairAtStart = true;
    public bool showCrosshairAtEnd = true;

    [Header("카메라")]
    public bool cameraMove = true;
    public bool trackPlayer = true;
    public Vector2 cameraPos;
    public float cameraZoom = 17;
    public float cameraMoveDuration = 0.5f;
    

    [Header("켜질 오브젝트")]
    public GameObject[] enableTargets;

    [Header("레터박스")]
    public bool canUseLetterBox = true;


    void Start()
    {
        float targetX = toThisObject ? transform.position.x : TargetPosX;
        // 三끼얏호우!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
        PlayerHandler.instance.PlayerMoveForwardTo(targetX);
        StartCoroutine(WaitForPlayerMove());

        if (hideCrosshairAtStart) CrosshairController.instance.ToggleSprite(false);
    }

    IEnumerator WaitForPlayerMove()
    {
        yield return null;
        yield return new WaitUntil(() => !PlayerHandler.instance.isPlayerBeingManipulated);
        MoveEnd();
    }

    void MoveEnd()
    {
        if (cameraMove)
        {
            if (trackPlayer) CameraMovement.TargetTracking(PlayerController.instance.transform, Vector3.up);
            else CameraMovement.DollyTo(cameraPos, cameraMoveDuration);

            CameraMovement.PositionZoom(cameraZoom, cameraMoveDuration);
        }


        if (hideCrosshairAtStart)
        {
            CrosshairController.instance.ToggleSprite(true);
        }

        if (enableTargets != null && enableTargets.Length != 0)
        {
            foreach (GameObject obj in enableTargets)
            {
                obj.SetActive(true);
            }
        }

        if (canUseLetterBox) LetterBoxController.Instance.SetEnable(false);

        if (showCrosshairAtEnd) CrosshairController.instance.ToggleSprite(true);

        Destroy(gameObject);
    }
}
