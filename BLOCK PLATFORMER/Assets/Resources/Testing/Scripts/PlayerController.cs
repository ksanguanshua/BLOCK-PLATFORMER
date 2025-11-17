using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField] float moveSpeed;
    [SerializeField] float jumpForce;

    Vector2 dir;
    Transform grabbableBox;
    Transform heldBox;

    Transform hand;
    [SerializeField]
    float handLerpSpeed;
    [SerializeField]
    float throwForce;
    
    Rigidbody2D rb;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        hand = transform.Find("Hand");
    }

    // Update is called once per frame
    void Update()
    {
        Move();
        Jump();
        HandMovement();
        if (heldBox == null)
        {
            Grab();
        }
        else
        {
            HeldBox();
        }
        
    }

    void Move()
    {
        float moveX = Input.GetAxisRaw("Horizontal");
        float moveY = Input.GetAxisRaw("Vertical");

        Vector2 moveVector = new Vector2(moveX, moveY);

        if (moveVector != Vector2.zero)
        {
            dir = moveVector;
        }

        rb.linearVelocityX = moveX * moveSpeed;
    }

    void HandMovement()
    {
        hand.position = Vector2.Lerp(hand.position, transform.position + (Vector3)dir, handLerpSpeed);
    }

    void Grab()
    {
        Collider2D coll = Physics2D.OverlapCircle(hand.position, hand.localScale.x);

        if (coll != null)
        {
            if (coll.CompareTag("Box"))
            {
                grabbableBox = coll.transform;
            }
            else
            {
                grabbableBox = null;
            }
        }
        else
        {
            grabbableBox = null;
        }

        if (grabbableBox != null)
        {
            hand.GetChild(0).position = grabbableBox.position;
            hand.GetChild(0).rotation = grabbableBox.rotation;
            hand.GetChild(0).GetComponent<SpriteRenderer>().enabled = true;

            if (Input.GetKey(KeyCode.LeftShift))
            {
                heldBox = grabbableBox;
                hand.GetChild(0).GetComponent<SpriteRenderer>().enabled = false;
                grabbableBox = null;
            }
        }
        else
        {
            hand.GetChild(0).GetComponent<SpriteRenderer>().enabled = false;
        }
    }

    void HeldBox()
    {
        heldBox.GetComponent<Collider2D>().enabled = false;
        heldBox.rotation = hand.rotation;
        heldBox.position = hand.position;
        heldBox.GetComponent<Rigidbody2D>().gravityScale = 0;

        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            heldBox.GetComponent<Collider2D>().enabled = true;
            heldBox.GetComponent<Rigidbody2D>().gravityScale = 2;
            heldBox.GetComponent<Rigidbody2D>().AddForce(new Vector2(dir.x, dir.y + 1) * throwForce, ForceMode2D.Impulse);
            heldBox = null;
        }
    }

    bool isGrounded()
    {
        RaycastHit2D[] hits = Physics2D.BoxCastAll(transform.position, transform.localScale, 0, Vector2.down, 0.1f);

        foreach( RaycastHit2D hit in hits)
        {
            if (hit.collider.gameObject.layer == LayerMask.NameToLayer("Ground"))
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
