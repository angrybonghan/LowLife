using System.Collections;
using UnityEngine;

public class TriggerBox : MonoBehaviour
{
    public enum TriggerBoxRangeType { cancellation, maintain, notUse }
    [Header("해제 범위")]
    public TriggerBoxRangeType rangeType = TriggerBoxRangeType.notUse;
    // 이 영역이 어떻게 사용될지에 대한 열거형
    // cancellation - 트리거박스의 해제 영역
    // maintain - 트리거박스의 유지 영역 (영역을 벗어났을 경우 해제)
    // notUse - 일회용. 스크립트가 한 번 작동 후 꺼짐. TriggerOut() 은 호출될 수 없음.

    public Vector2 hitboxOffset = Vector2.zero;    // 오프셋
    public Vector2 hitboxSize = new Vector2(1.0f, 1.0f); // 크기 (width, height)
    public LayerMask playerLayer;

    I_TriggerBox[] TBP; // TBP : [T]rigger [B]ox [P]arts

    bool isEnabled = false;
    Vector2 boxCenter;

    void Start()
    {
        Collider2D col = GetComponent<Collider2D>();
        if (col == null)
        {
            Destroy(gameObject);
            this.enabled = false;
            return;
        }
        col.isTrigger = true;

        TBP = gameObject.GetComponents<I_TriggerBox>();
        
        if (TBP.Length <= 0)
        {
            Destroy(gameObject);
            this.enabled = false;
            return;
        }

        boxCenter = (Vector2)transform.position + hitboxOffset;
    }

    IEnumerator TriggerCancellation()
    {
        if (rangeType == TriggerBoxRangeType.cancellation)
        {
            yield return new WaitUntil(() => IsPlayerInRange());
        }
        else
        {
            yield return new WaitUntil(() => !IsPlayerInRange());
        }

        TriggerParts(false);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (isEnabled) return;

        if (other.CompareTag("Player"))
        {
            TriggerParts(true);
            if (rangeType == TriggerBoxRangeType.notUse)
            {
                this.enabled = false;
                return;
            }
            else
            {
                StartCoroutine(TriggerCancellation());
            }
        }
    }

    void TriggerParts(bool In)
    {
        // 성능을 개선하고 가독성을 줄이는 것을 택함

        isEnabled = In;
        if (In)
        {
            foreach (I_TriggerBox part in TBP)
            {
                part.TriggerIn();
            }
        }
        else
        {
            foreach (I_TriggerBox part in TBP)
            {
                part.TriggerOut();
            }
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
