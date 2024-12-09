using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private Rigidbody2D rb;
    private Animator animator;
    private SpriteRenderer spriteRenderer;

    public float walkSpeed = 2f;
    public float runSpeed = 4f;
    public float jumpForce = 5f;

    private bool isRunning = false;
    private bool isGrounded = false;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        float moveInput = Input.GetAxis("Horizontal");

        // Movimiento hacia la derecha
        if (moveInput > 0)
        {
            if (Input.GetKeyDown(KeyCode.D))
            {
                isRunning = true;
                animator.SetBool("isRunning", true);
            }

            if (isRunning)
            {
                rb.linearVelocity = new Vector2(runSpeed * moveInput, rb.linearVelocity.y);
            }
            else
            {
                rb.linearVelocity = new Vector2(walkSpeed * moveInput, rb.linearVelocity.y);
                animator.SetBool("isWalking", true);
            }

            spriteRenderer.flipX = false;
        }

        // Movimiento hacia la izquierda
        else if (moveInput < 0)
        {
            if (Input.GetKeyDown(KeyCode.A))
            {
                isRunning = true;
                animator.SetBool("isRunning", true);
            }

            if (isRunning)
            {
                rb.linearVelocity = new Vector2(runSpeed * moveInput, rb.linearVelocity.y);
            }
            else
            {
                rb.linearVelocity = new Vector2(walkSpeed * moveInput, rb.linearVelocity.y);
                animator.SetBool("isWalking", true);
            }

            spriteRenderer.flipX = true;
        }
        else
        {
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
            animator.SetBool("isWalking", false);
            animator.SetBool("isRunning", false);
        }

        // Salto
        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            animator.SetBool("isJumping", true);
        }

        // Ataque Neutral
        if (Input.GetMouseButtonDown(0))
        {
            animator.SetBool("isNeutralAttack", true);
        }
        else
        {
            animator.SetBool("isNeutralAttack", false);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "Ground")
        {
            isGrounded = true;
            animator.SetBool("isJumping", false);
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "Ground")
        {
            isGrounded = false;
        }
    }
}