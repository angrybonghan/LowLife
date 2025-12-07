using System.Collections;
using UnityEngine;
//using static UnityEditor.FilePathAttribute;

[RequireComponent(typeof(Animator),typeof(Collider2D))]
public class Spencer_SS0 : MonoBehaviour, I_Attackable
{
    [Header("시간")]
    public float teleportTime = 1.0f;

    [Header("위치")]
    public float centerX;
    public float positionSpacing;

    [HideInInspector] public bool useUnconditionallyLocation = false;
    [HideInInspector] public int unconditionallyLocationNumber = -1;


    Animator anim;
    Collider2D col;

    private void Awake()
    {
        anim = GetComponent<Animator>();
        col = GetComponent<Collider2D>();
    }

    private void Start()
    {
        if (SpencerManager.Instance.halfHP)
        {
            teleportTime = 0.35f;
        }
    }

    public void StartTeleport()
    {
        transform.position = GetTeleportPos();
        col.enabled = false;
        StartCoroutine(WaitTeleportDutation());
    }

    IEnumerator WaitTeleportDutation()
    {
        yield return new WaitForSeconds(teleportTime);
        anim.SetTrigger("tp");
    }

    public void EndTeleport()
    {
        col.enabled = true;
        SpencerManager.Instance.canUseSklill = true;
    }

    Vector2 GetTeleportPos()
    {
        Vector2 pos = transform.position;
        pos.x = centerX;

        if (useUnconditionallyLocation)
        {
            if (unconditionallyLocationNumber == 1)
            {
                pos.x -= positionSpacing;
                SpencerManager.Instance.positionNumber = 1;
                return pos;
            }
            else if (unconditionallyLocationNumber == 3)
            {
                pos.x += positionSpacing;
                SpencerManager.Instance.positionNumber = 3;
                return pos;
            }
            else
            {
                SpencerManager.Instance.positionNumber = 2;
                return pos;
            }
        }

        if (SpencerManager.Instance.positionNumber == 2)
        {
            if (Random.value > 0.5f)
            {
                pos.x += positionSpacing;
                SpencerManager.Instance.positionNumber = 3;
            }
            else
            {
                pos.x -= positionSpacing;
                SpencerManager.Instance.positionNumber = 1;
            }
        }
        else
        {
            if (Random.value > 0.7f)
            {
                if (SpencerManager.Instance.positionNumber == 1)
                {
                    pos.x += positionSpacing;
                    SpencerManager.Instance.positionNumber = 3;
                }
                else
                {
                    pos.x -= positionSpacing;
                    SpencerManager.Instance.positionNumber = 1;
                }
            }
            else
            {
                SpencerManager.Instance.positionNumber = 2;
            }
        }

        return pos;
    }

    public bool CanAttack(Transform attacker)
    {
        return true;
    }

    public void OnAttack(Transform attacker)
    {
        SpencerManager.Instance.TakeDamage();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            PlayerController.instance.ImmediateDeath();
        }
    }
}
