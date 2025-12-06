using UnityEngine;

public class WallKickTutorial : MonoBehaviour
{
    [Header("tmp")]
    public TMP_Animator Text;
    public float transitionTime;

    [Header("플레이어 범위")]
    public Vector2 hitboxOffset = Vector2.zero;    // 오프셋
    public Vector2 hitboxSize = new Vector2(1.0f, 1.0f); // 크기 (width, height)
    public LayerMask playerLayer;

    [Header("색상")]
    public Color disableColor;
    public Color activateColor;

    [Header("텍스트")]
    public string text1;
    public string text2;

    [Header("크기")]
    public float textSize1;
    public float textSize2;


    bool activateText = false;
    Vector2 boxCenter;

    void Start()
    {
        boxCenter = (Vector2)transform.position + hitboxOffset;
    }

    void Update()
    {
        if (IsPlayerInRange())
        {
            TextHandler();
        }
        else if (activateText)
        {
            Disable();
        }
    }

    void TextHandler()
    {
        if (activateText)
        {
            if (!PlayerController.instance.isWallSliding)
            {
                Disable();
            }
        }
        else
        {
            if (PlayerController.instance.isWallSliding)
            {
                Enable();
            }
        }
    }

    void Enable()
    {
        activateText = true;

        Text.SetColor(activateColor, transitionTime);
        Text.SetText(text2);
        Text.SetSize(textSize2, transitionTime);
    }

    private void Disable()
    {
        activateText = false;

        Text.SetColor(disableColor, transitionTime);
        Text.SetText(text1);
        Text.SetSize(textSize1, transitionTime);
    }

    public bool IsPlayerInRange()
    {
        Collider2D hitCollider = Physics2D.OverlapBox(
            boxCenter,
            hitboxSize,
            0f,
            playerLayer
        );

        if (hitCollider != null && hitCollider.CompareTag("Player"))
        {
            return true;
        }

        return false;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;

        Vector2 hitboxGizmoCenter = (Vector2)transform.position + hitboxOffset;
        Gizmos.DrawWireCube(hitboxGizmoCenter, hitboxSize);
    }
}
