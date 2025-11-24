using System.Collections;
using UnityEngine;

public class ShieldLeapTutorial : MonoBehaviour
{
    [Header("텍스트")]
    public TMP_Animator[] Texts;
    public float transitionTime;

    [Header("플레이어 범위")]
    public Vector2 hitboxOffset = Vector2.zero;    // 오프셋
    public Vector2 hitboxSize = new Vector2(1.0f, 1.0f); // 크기 (width, height)
    public LayerMask playerLayer;

    [Header("색상")]
    public Color weakColor;
    public Color strongColor;

    [Header("이미지")]
    public Sprite_Animator[] sprites;

    bool activateShieldLeapText = false;
    bool hasPlayerEntered = false;
    Vector2 boxCenter;

    PlayerController player;


    void Start()
    {
        Texts[0].SetSize(8, transitionTime);
        Texts[0].SetColor(strongColor, transitionTime);
        Texts[1].SetSize(6, transitionTime);
        Texts[1].SetColor(weakColor, transitionTime);
        activateShieldLeapText = false;

        player = PlayerController.instance;
        boxCenter = (Vector2)transform.position + hitboxOffset;

        foreach (Sprite_Animator sprite in sprites)
        {
            sprite.SetAlpha(0f, 0f);
        }
    }

    private void Update()
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
        else if (activateShieldLeapText)
        {
            Texts[0].SetSize(8, transitionTime);
            Texts[0].SetColor(strongColor, transitionTime);
            Texts[1].SetSize(6, transitionTime);
            Texts[1].SetColor(weakColor, transitionTime);
            activateShieldLeapText = false;
        }
    }

    IEnumerator ShowSprite()
    {
        yield return new WaitForSeconds(3.5f);
        foreach (Sprite_Animator sprite in sprites)
        {
            sprite.SetAlpha(0.15f, 0.5f);
        }
    }

    void TextHandler()
    {
        if (ShieldMovement.shieldInstance != null && !player.hasShield && !ShieldMovement.shieldInstance.isReturning)
        {
            if (!activateShieldLeapText)
            {
                Texts[0].SetSize(6, transitionTime);
                Texts[0].SetColor(weakColor, transitionTime);
                Texts[1].SetSize(8, transitionTime);
                Texts[1].SetColor(strongColor, transitionTime);
                activateShieldLeapText = true;
            }
        }
        else if (activateShieldLeapText)
        {
            Texts[0].SetSize(8, transitionTime);
            Texts[0].SetColor(strongColor, transitionTime);
            Texts[1].SetSize(6, transitionTime);
            Texts[1].SetColor(weakColor, transitionTime);
            activateShieldLeapText = false;
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
