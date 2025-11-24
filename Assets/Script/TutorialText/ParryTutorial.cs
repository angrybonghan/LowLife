using System.Collections;
using UnityEngine;

public class ParryTutorial : MonoBehaviour
{
    [Header("텍스트")]
    public TMP_Animator[] Texts;
    public float transitionTime;

    [Header("플레이어 범위")]
    public Vector2 hitboxOffset = Vector2.zero;    // 오프셋
    public Vector2 hitboxSize = new Vector2(1.0f, 1.0f); // 크기 (width, height)
    public LayerMask playerLayer;

    [Header("색상")]
    public Color disableColor1;
    public Color disableColor2;
    public Color activateColor1;
    public Color activateColor2;

    [Header("이미지")]
    public Sprite_Animator[] sprites;

    bool activateText = false;
    bool hasPlayerEntered = false;
    Vector2 boxCenter;

    PlayerController player;

    void Start()
    {
        Texts[0].SetColor(disableColor1, transitionTime);
        Texts[1].SetSize(6, transitionTime);
        Texts[1].SetColor(disableColor2, transitionTime);
        activateText = false;

        player = PlayerController.instance;
        boxCenter = (Vector2)transform.position + hitboxOffset;

        foreach (Sprite_Animator sprite in sprites)
        {
            sprite.SetAlpha(0f, 0f);
        }
    }

    void Update()
    {
        if (IsPlayerInRange())
        {
            TextHandler();
            if (!hasPlayerEntered)
            {
                StartCoroutine(ShowSprite());
                hasPlayerEntered = true;
            }
        }
        else if (activateText)
        {
            Texts[0].SetColor(disableColor1, transitionTime);
            Texts[1].SetColor(disableColor2, transitionTime);
            Texts[1].SetSize(6, transitionTime);
            activateText = false;
        }
    }

    
    IEnumerator ShowSprite()
    {
        yield return new WaitForSeconds(1.5f);
        foreach (Sprite_Animator sprite in sprites)
        {
            sprite.SetAlpha(0.15f, 0.5f);
        }
    }

    void TextHandler()
    {
        if (!activateText)
        {
            if (player.isShielding)
            {
                Texts[0].SetColor(activateColor1, transitionTime);
                Texts[1].SetColor(activateColor2, transitionTime);
                Texts[1].SetSize(8, transitionTime);
                activateText = true;
            }
        }
        else if (!player.isShielding)
        {
            Texts[0].SetColor(disableColor1, transitionTime);
            Texts[1].SetColor(disableColor2, transitionTime);
            Texts[1].SetSize(6, transitionTime);
            activateText = false;
        }
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
