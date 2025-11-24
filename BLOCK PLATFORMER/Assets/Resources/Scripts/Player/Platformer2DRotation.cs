using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D), typeof(Platformer2DMovementBase))]
public class Platformer2DRotation : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 20f;         // top speed along slope/tangent
    public float acceleration = 1f;       // how quickly we approach target speed on ground
    public float airAcceleration = 0.1f;  // how quickly we approach target speed in air
    public float jumpForce = 10f;

    [Header("Ground Check")]
    public float groundCheckDistance = 0.6f;      // for slope‐aligned ground detection
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
    Platformer2DMovementBase c_mb;
    bool wasGroundedLastFrame;

    void Start()
    {
        c_mb = GetComponent<Platformer2DMovementBase>();
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

    public void SlopeGrounded()
    {
        if (c_mb.ss_isGrounded && c_mb.in_movementInput.x != 0)
        {
            rb.AddForce(-c_mb.groundNormal * downForce, ForceMode2D.Force);
        }
    }

    public void UpdateGrounded()
    {
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
            if (Mathf.Approximately(c_mb.in_movementInput.x, 0f))
            {
                // No horizontal input → higher friction (0.5)
                col.sharedMaterial = highFriction;
            }
            else
            {
                // Player is pressing left/right → zero friction
                col.sharedMaterial = zeroFriction;
            }
        }
        else
        {
            // In the air (or not truly on flat ground) → zero friction
            col.sharedMaterial = zeroFriction;
        }
        // Raycast “down” along the player’s local down to detect slope/curved ground
        RaycastHit2D hit = Physics2D.Raycast(
            transform.position,
            -transform.up,
            groundCheckDistance,
            groundLayer
        );
        if (hit.collider != null)
        {
            c_mb.ss_isGrounded = true;
            c_mb.groundNormal = hit.normal;

            // If we just landed this frame (wasn't grounded last frame, now c_mb.ss_isGrounded)
            if (!wasGroundedLastFrame)
            {
                // Project whatever velocity we had onto the new tangent
                Vector2 tangent = new Vector2(c_mb.groundNormal.y, -c_mb.groundNormal.x).normalized;
                float incomingSpeedAlongTangent = Vector2.Dot(rb.linearVelocity, tangent);

                // “Translate” downward velocity into slope-run velocity
                rb.AddForce(tangent * incomingSpeedAlongTangent,ForceMode2D.Impulse);
            }
        }
        else
        {
            c_mb.ss_isGrounded = false;
            c_mb.groundNormal = Vector2.up; // fallback if airborne
        }
    }

    public void RotateToGroundOrUpright()
    {
        // Find the slope tangent direction
        Vector2 tangent = new Vector2(c_mb.groundNormal.y, -c_mb.groundNormal.x).normalized;
        float speedAlongTangent = c_mb.lastTangentialSpeed;

        // If slope‐grounded AND speed > threshold, align to slope normal
        if (c_mb.ss_isGrounded && Mathf.Abs(speedAlongTangent) > rotationResetThreshold)
        {
            // Compute target rotation so transform.up aligns with groundNormal
            Quaternion targetRotation = Quaternion.FromToRotation(transform.up, c_mb.groundNormal) * transform.rotation;
            float zAngle = Mathf.LerpAngle(
                rb.rotation,
                targetRotation.eulerAngles.z,
                15f * Time.fixedDeltaTime
            );
            rb.MoveRotation(zAngle);
        }
        else
        {
            // Too slow (or not on slope) → rotate back upright (0 z-rotation)
            float zAngle = Mathf.LerpAngle(rb.rotation, 0f, 5f * Time.fixedDeltaTime);
            rb.MoveRotation(zAngle);
        }

        // If your sprite lives on a child, force it to match rotation
        if (spriteTransform != null)
        {
            spriteTransform.rotation = transform.rotation;
        }
        wasGroundedLastFrame = c_mb.ss_isGrounded;
    }

    // Draw debug lines in the Scene View
    void OnDrawGizmos()
    {
        if (!Application.isPlaying) return;

        // 1) Draw groundNormal (green)
        Gizmos.color = Color.green;
        Gizmos.DrawLine(transform.position, transform.position + (Vector3)c_mb.groundNormal);

        // 2) Draw current tangent facing direction (blue)
        Vector2 tangent = new Vector2(c_mb.groundNormal.y, -c_mb.groundNormal.x).normalized * Mathf.Sign(c_mb.in_movementInput.x);
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
