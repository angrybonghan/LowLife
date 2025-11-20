using UnityEngine;
using System.Collections;

[RequireComponent(typeof(SpriteRenderer), typeof(BoxCollider2D))]
public class HiddenSpace : MonoBehaviour
{
    public float fadeSpeed = 1f;

    private SpriteRenderer spriteRenderer;
    private Coroutine fadeCoroutine;

    private float targetAlpha;

    private bool hasPlayer = false;

    private const string PLAYER_TAG = "Player";

    GameObject playerObject;    // 플레이어 오브젝트

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void Start()
    {
        SetAlpha(1);
        playerObject = PlayerController.instance.gameObject;
        hasPlayer = playerObject != null;

        BoxCollider2D boxCol = GetComponent<BoxCollider2D>();
        boxCol.isTrigger = true;
    }

    private void Update()
    {
        if (hasPlayer && playerObject == null)
        {
            StopAllCoroutines();
            hasPlayer = false;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag(PLAYER_TAG))
        {
            targetAlpha = 0.0f;

            StopCurrentFadeCoroutine();
            fadeCoroutine = StartCoroutine(FadeToTargetAlpha());
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag(PLAYER_TAG))
        {
            targetAlpha = 1.0f;

            StopCurrentFadeCoroutine();
            if (gameObject.activeInHierarchy)
            {
                fadeCoroutine = StartCoroutine(FadeToTargetAlpha());
            }
        }
    }
    private void StopCurrentFadeCoroutine()
    {
        if (fadeCoroutine != null)
        {
            StopCoroutine(fadeCoroutine);
        }
    }

    private IEnumerator FadeToTargetAlpha()
    {
        while (!Mathf.Approximately(spriteRenderer.color.a, targetAlpha))
        {
            float currentAlpha = spriteRenderer.color.a;

            float newAlpha = Mathf.MoveTowards(
                currentAlpha,
                targetAlpha,
                fadeSpeed * Time.deltaTime
            );

            SetAlpha(newAlpha);
            yield return null;
        }

        // 반복문 종료 후 최종적으로 목표 알파값으로 설정합니다.
        SetAlpha(targetAlpha);

        fadeCoroutine = null;
    }

    void SetAlpha(float alpha)
    {
        Color currentColor = spriteRenderer.color;
        currentColor.a = alpha;
        spriteRenderer.color = currentColor;
    }
}