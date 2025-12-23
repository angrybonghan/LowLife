using System.Collections;
using UnityEngine;

public class Maw_SS0 : MonoBehaviour, I_MawSkill
{
    [Header("소리")]
    public AudioClip growlSound;

    [Header("파티클")]
    public Transform particlePos;
    public GameObject particlePrefab;
    public float particleInterval = 0.2f;

    [Header("중앙 X")]
    public float centerX;
    
    public bool isFacingRight { get; set; }
    Coroutine particleCoroutine;

    private void Start()
    {
        if (!isFacingRight)
        {
            transform.localScale = new Vector3(transform.localScale.x * -1, transform.localScale.y, transform.localScale.z);
        }

        LookPos(centerX);
    }

    public void StartParticle()
    {
        particleCoroutine = StartCoroutine(SpawnParticle());
    }

    public void StopParticle()
    {
        if (particleCoroutine != null)
        {
            StopCoroutine(particleCoroutine);
        }
    }

    IEnumerator SpawnParticle()
    {
        while (true)
        {
            Instantiate(particlePrefab, particlePos.position, Quaternion.identity);
            yield return new WaitForSeconds(particleInterval);
        }
    }

    public void PlayGrowlSound()
    {
        SoundManager.instance.PlaySoundAtPosition(transform.position, growlSound);
    }

    public void EndAnimation()
    {
        MawManager.instance.canUseSklill = true;
    }


    void LookPos(float targetX)
    {
        float directionX = targetX - transform.position.x;

        if (directionX != 0 && (directionX > 0) != isFacingRight)
        {
            Flip();
        }
    }

    public void Flip()
    {
        MawManager.instance.isFacingRight = isFacingRight = !isFacingRight;
        transform.localScale = new Vector3(transform.localScale.x * -1, transform.localScale.y, transform.localScale.z);
    }

    public Transform GetTransform()
    {
        return transform;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            PlayerController.instance.ImmediateDeath();
        }
    }
}
