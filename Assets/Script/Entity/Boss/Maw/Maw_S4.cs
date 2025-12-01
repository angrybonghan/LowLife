using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Animator))]
public class Maw_S4 : MonoBehaviour, I_MawSkill
{
    [Header("점프 목표")]
    public float targetY;

    public bool isFacingRight { get; set; }

    Animator anim;
    Rigidbody2D rb;

    private void Awake()
    {
        anim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
    }

    void Start()
    {
        if (!isFacingRight)
        {
            transform.localScale = new Vector3(transform.localScale.x * -1, transform.localScale.y, transform.localScale.z);
        }
    }


    void Update()
    {
        
    }

    IEnumerator SkillSequence()
    {
        yield return null;
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
}
