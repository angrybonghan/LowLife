using System.Collections;
using UnityEngine;

public class GameSceneStart : MonoBehaviour
{
    [Header("이동")]
    public bool toThisObject = false;
    public float TargetPosX = 0;

    [Header("카메라")]
    public bool cameraMove = true;
    public float cameraZoom = 17;
    public float cameraZoomduration = 0.5f;

    [Header("조준점")]
    public bool ToggleCrosshair = true;

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

        if (ToggleCrosshair)
        {
            CrosshairController.instance.ToggleSprite(false);
        }
    }

    IEnumerator WaitForPlayerMove()
    {
        yield return null;
        yield return new WaitUntil(() => !PlayerHandler.instance.isPlayerBeingManipulated);
        MoveEnd();
    }

    void MoveEnd()
    {
        CameraMovement.PositionZoom(cameraZoom, cameraZoomduration);
        CameraMovement.TargetTracking(PlayerController.instance.transform, new Vector3(0, 1f, 0));

        if (ToggleCrosshair)
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

        if (canUseLetterBox)
        {
            LetterBoxController.Instance.SetEnable(false);
        }

        Destroy(gameObject);
    }
}
