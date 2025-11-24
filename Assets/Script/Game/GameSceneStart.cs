using System.Collections;
using UnityEngine;

public class GameSceneStart : MonoBehaviour
{
    [Header("ÀÌµ¿")]
    public bool toThisObject = false;
    public float TargetPosX = 0;

    [Header("Ä«¸Þ¶ó")]
    public bool cameraMove = true;
    public float cameraZoom = 17;
    public float cameraZoomduration = 0.5f;


    void Start()
    {
        float targetX = toThisObject ? transform.position.x : TargetPosX;
        // ß²³¢¾æÈ£¿ì!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
        PlayerHandler.instance.PlayerMoveForwardTo(targetX);
        StartCoroutine(WaitForPlayerMove());
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

        Destroy(gameObject);
    }
}
