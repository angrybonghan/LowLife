using UnityEngine;

public class StageEnd : MonoBehaviour
{
    [Header("이동 위치")]
    public bool canPlayerMove = true;
    public float targetX = 0f;

    [Header("레터박스")]
    public bool useLetterBox = true;
    public string text = "계속...";

    [Header("카메라")]
    public bool canMoveCamera = true;
    public Vector2 cameraPos = Vector2.zero;
    public float zoom = 17f;
    public float moveDuration = 2f;

    [Header("스테이지")]
    public float WaitTime1 = 1f;
    public float fadeOutTime = 1.2887f;
    public string loadingScene = "StageLoading_1";
    public string nextScene = "Swomp_2";

    [Header("비활성화될 오브젝트")]
    public GameObject[] disableAtAction;

    bool canAction = true;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!canAction) return;

        if (collision.CompareTag("Player"))
        {
            canAction = false;

            CrosshairController.instance.ToggleSprite(false);

            PlayerController.instance.AllStop();
            if (canPlayerMove) PlayerHandler.instance.PlayerMoveForwardTo(targetX);

            if (canMoveCamera)
            {
                CameraMovement.DollyTo(cameraPos, moveDuration);
                CameraMovement.PositionZoom(zoom, moveDuration);
            }

            AchievementManager.Instance.OnSceneEntered(nextScene);
            ScreenTransition.ScreenTransitionGoto(nextScene, loadingScene, Color.black, WaitTime1, fadeOutTime, 2, 0.5f, 0);

            foreach (GameObject obj in disableAtAction)
            {
                obj.SetActive(false);
            }

            if (useLetterBox && LetterBoxController.Instance != null)
            {
                LetterBoxController.Instance.SetEnable(true);
                LetterBoxController.Instance.SetText(text);
            }
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(cameraPos, 0.35f);
    }
}
