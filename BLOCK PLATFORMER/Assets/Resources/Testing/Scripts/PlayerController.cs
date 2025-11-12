using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField] float moveSpeed;
    [SerializeField] float jumpForce;
    
    Rigidbody2D rb;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void Update()
    {
        Move();
        Jump();
    }

    void Move()
    {
        float moveX = Input.GetAxisRaw("Horizontal");

        rb.linearVelocityX = moveX * moveSpeed;
    }

    bool isGrounded()
    {
        RaycastHit2D[] hits = Physics2D.BoxCastAll(transform.position, transform.localScale, 0, Vector2.down, 0.1f);

        foreach( RaycastHit2D hit in hits)
        {
            if (hit.collider.CompareTag("Ground"))
            {
                return true;
            }
        }

        return false;
    }

    void Jump()
    {
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded())
        {
            rb.AddForceY(jumpForce, ForceMode2D.Impulse);
        }
    }
}
