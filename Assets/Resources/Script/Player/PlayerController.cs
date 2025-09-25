using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    public float acceleration = 5;
    public float maxSpeed = 15;
    public float jumpForce = 16;


    private float moveInput = 0;
    private float currentMoveSpeed = 1;

    private bool isFacingRight = true;
    private bool isRunning = false;

    private Rigidbody2D rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }



    void Update()
    {
        InputHandler();
        CheckFlip();
        HandleMovement();
    }

    void DirectionHandler()
    {

    }

    void CheckFlip()
    {
        if ((moveInput > 0 && !isFacingRight) || (moveInput < 0 && isFacingRight)) Flip();
    }

    void Flip()
    {
        isFacingRight = !isFacingRight;
        transform.localScale = new Vector3(transform.localScale.x * -1, transform.localScale.y, transform.localScale.z);
    }

    void InputHandler()
    {
        if (IsSingleInput())
        {
            if (Input.GetKey(KeyCode.A)) moveInput = -1;
            else moveInput = 1;

            isRunning = true;
        }
        else
        {
            moveInput = 0;

            isRunning = false;
        }
    }

    public bool IsSingleInput()
    {
        bool notBothKeys = !(Input.GetKey(KeyCode.A) && Input.GetKey(KeyCode.D));
        bool atLeastOneKey = (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.D));

        return notBothKeys && atLeastOneKey;
    }

    void HandleMovement()
    {
        if (isRunning)
        {
            currentMoveSpeed = Mathf.Min(currentMoveSpeed + acceleration * Time.deltaTime, maxSpeed);
        }
        else
        {
            currentMoveSpeed = Mathf.Max(currentMoveSpeed - acceleration * Time.deltaTime, 1);
        }

        rb.velocity = new Vector2(moveInput * currentMoveSpeed, rb.velocity.y);
    }
}
