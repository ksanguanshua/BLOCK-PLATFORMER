using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D), typeof(Platformer2DMovementBase))]
public class Platformer2DRotationBase : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 20f;         // top speed along slope/tangent
    public float acceleration = 1f;       // how quickly we approach target speed on ground
    public float airAcceleration = 0.1f;  // how quickly we approach target speed in air
    public float jumpForce = 10f;

    [Header("Ground Check")]
    public float groundCheckDistance = 0.6f;      // for slope?aligned ground detection
    public LayerMask groundLayer;

    [Header("True Ground Check")]
    public float trueGroundCheckDistance = 0.1f;  // small offset straight down
    [HideInInspector] public bool trueGrounded;   // set by a Vector2.down raycast

    [Header("Downward Stick Force")]
    public float downForce = 10f;
    // extra force pushing into the ground when grounded;
    // removed when airborne.

    [Header("Friction Materials")]
    public PhysicsMaterial2D highFriction;  // friction = 0.5
    public PhysicsMaterial2D zeroFriction;  // friction = 0

    [Header("Rotation Reset")]
    public float rotationResetThreshold = 0.5f;
    // if |tangential velocity| < this AND grounded, we smoothly rotate back upright

    [Header("Sprite (Optional)")]
    public Transform spriteTransform;
    // If your SpriteRenderer is on a child, assign that transform here.

    Rigidbody2D rb;
    Collider2D col;

    Vector2 moveInput;         // normalized (-1, 0, or 1) on X
    bool isGrounded;
    bool wasGroundedLastFrame;

    Vector2 groundNormal = Vector2.up;
    float lastTangentialSpeed = 0f;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();

        // If spriteTransform wasn’t assigned manually, try to find a child SpriteRenderer
        if (spriteTransform == null)
        {
            SpriteRenderer sr = GetComponentInChildren<SpriteRenderer>();
            if (sr != null)
                spriteTransform = sr.transform;
        }
    }

    void Update()
    {
        // Read raw horizontal input each frame
        float h = Input.GetAxisRaw("Horizontal");
        moveInput = new Vector2(h, 0);

        // Jump only if •isGrounded• (slope check) OR •trueGrounded• could also be used here
        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            rb.AddForce(groundNormal * jumpForce, ForceMode2D.Impulse);
        }
    }

    void FixedUpdate()
    {
        // 1) Update slope?based grounded info
        UpdateGrounded();

        // 1.1) Update trueGrounded (always cast straight down)
        RaycastHit2D downHit = Physics2D.Raycast(
            transform.position,
            Vector2.down,
            trueGroundCheckDistance,
            groundLayer
        );
        trueGrounded = downHit.collider != null;

        // 1.5) Adjust friction based on trueGrounded & input
        if (trueGrounded)
        {
            if (Mathf.Approximately(moveInput.x, 0f))
            {
                // No horizontal input ? higher friction (0.5)
                col.sharedMaterial = highFriction;
            }
            else
            {
                // Player is pressing left/right ? zero friction
                col.sharedMaterial = zeroFriction;
            }
        }
        else
        {
            // In the air (or not truly on flat ground) ? zero friction
            col.sharedMaterial = zeroFriction;
        }

        // 2) Build movement along slope tangent
        Vector2 tangent = new Vector2(groundNormal.y, -groundNormal.x).normalized;
        float targetSpeed = moveInput.x * moveSpeed;
        float currentSpeed = Vector2.Dot(rb.linearVelocity, tangent);
        lastTangentialSpeed = currentSpeed;

        // Choose acceleration based on whether player is on ground
        float useAccel = isGrounded ? acceleration : airAcceleration;

        // How much we need to change speed by this frame
        float speedDiff = targetSpeed - currentSpeed;
        Vector2 force = tangent * speedDiff * useAccel;
        rb.AddForce(force, ForceMode2D.Force);

        // 2.5) If slope?grounded, push player “into” the surface to keep them stuck
        if (isGrounded && moveInput.x != 0)
        {
            rb.AddForce(-groundNormal * downForce, ForceMode2D.Force);
        }

        // 3) Rotate based on groundNormal—but reset to upright if too slow
        RotateToGroundOrUpright();

        // Track grounded state for next FixedUpdate
        wasGroundedLastFrame = isGrounded;
    }

    void UpdateGrounded()
    {
        // Raycast “down” along the player’s local down to detect slope/curved ground
        RaycastHit2D hit = Physics2D.Raycast(
            transform.position,
            -transform.up,
            groundCheckDistance,
            groundLayer
        );
        if (hit.collider != null)
        {
            isGrounded = true;
            groundNormal = hit.normal;

            // If we just landed this frame (wasn't grounded last frame, now isGrounded)
            if (!wasGroundedLastFrame)
            {
                // Project whatever velocity we had onto the new tangent
                Vector2 tangent = new Vector2(groundNormal.y, -groundNormal.x).normalized;
                float incomingSpeedAlongTangent = Vector2.Dot(rb.linearVelocity, tangent);

                // “Translate” downward velocity into slope-run velocity
                rb.linearVelocity = tangent * incomingSpeedAlongTangent;
            }
        }
        else
        {
            isGrounded = false;
            groundNormal = Vector2.up; // fallback if airborne
        }
    }

    void RotateToGroundOrUpright()
    {
        // Find the slope tangent direction
        Vector2 tangent = new Vector2(groundNormal.y, -groundNormal.x).normalized;
        float speedAlongTangent = lastTangentialSpeed;

        // If slope?grounded AND speed > threshold, align to slope normal
        if (isGrounded && Mathf.Abs(speedAlongTangent) > rotationResetThreshold)
        {
            // Compute target rotation so transform.up aligns with groundNormal
            Quaternion targetRotation = Quaternion.FromToRotation(transform.up, groundNormal) * transform.rotation;
            float zAngle = Mathf.LerpAngle(
                rb.rotation,
                targetRotation.eulerAngles.z,
                15f * Time.fixedDeltaTime
            );
            rb.MoveRotation(zAngle);
        }
        else
        {
            // Too slow (or not on slope) ? rotate back upright (0 z-rotation)
            float zAngle = Mathf.LerpAngle(rb.rotation, 0f, 5f * Time.fixedDeltaTime);
            rb.MoveRotation(zAngle);
        }

        // If your sprite lives on a child, force it to match rotation
        if (spriteTransform != null)
        {
            spriteTransform.rotation = transform.rotation;
        }
    }

    // Draw debug lines in the Scene View
    void OnDrawGizmos()
    {
        if (!Application.isPlaying) return;

        // 1) Draw groundNormal (green)
        Gizmos.color = Color.green;
        Gizmos.DrawLine(transform.position, transform.position + (Vector3)groundNormal);

        // 2) Draw current tangent facing direction (blue)
        Vector2 tangent = new Vector2(groundNormal.y, -groundNormal.x).normalized * Mathf.Sign(moveInput.x);
        Gizmos.color = Color.blue;
        Gizmos.DrawLine(transform.position, transform.position + (Vector3)tangent);

        // 3) Draw trueGround check (red downward ray)
        Gizmos.color = Color.red;
        Gizmos.DrawLine(
            transform.position,
            transform.position + Vector3.down * trueGroundCheckDistance
        );
    }
}
