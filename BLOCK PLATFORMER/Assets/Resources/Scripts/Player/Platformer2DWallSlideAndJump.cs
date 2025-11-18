using UnityEngine;

[RequireComponent(typeof(Platformer2DMovementBase))]
public class Platformer2DWallSlideAndJump : MonoBehaviour
{
    #region Modifiers
    [Header("Wall Slide Settings")]
    [Range(0.0f, 25.0f)][SerializeField] public float m_slideSpeed = 2f;
    [SerializeField] private bool AUTO_LOCK_ON = true; // Automatically start sliding when near a wall

    [Header("Wall Jump Settings")]
    [Range(0.0f, 50.0f)][SerializeField] private float m_wallJumpForce = 10f;
    [Range(0.0f, 100.0f)][SerializeField] private float m_wallJumpAngle = 1f;
    [SerializeField] private bool SLIDING_NECESSARY = true; // Wall sliding required for wall jumping

    [Header("Wall Interaction")]
    [SerializeField] private float m_wallCheckDistance = 0.5f;
    [SerializeField] private LayerMask lm_wallLayer;

    [Header("Debug")]
    [SerializeField] private bool debugWallInteraction = false;
    #endregion

    #region Components
    private Rigidbody2D c_rb;
    private Platformer2DMovementBase c_movementBase;
    private bool ch_isTouchingWall;
    private bool ch_isSliding;
    private int currentWallSide; // -1 for left, 1 for right
    #endregion

    #region Initialization
    private void Start()
    {
        ComponentGrab();
    }

    void ComponentGrab()
    {
        c_rb = GetComponent<Rigidbody2D>();
        c_movementBase = GetComponent<Platformer2DMovementBase>();
    }
    #endregion

    #region Wall Sliding
    public void WallCheck()
    {
        Vector2 leftWallCheck = Vector2.left;
        Vector2 rightWallCheck = Vector2.right;

        bool leftWall = Physics2D.Raycast(transform.position, leftWallCheck, m_wallCheckDistance, lm_wallLayer);
        bool rightWall = Physics2D.Raycast(transform.position, rightWallCheck, m_wallCheckDistance, lm_wallLayer);

        ch_isTouchingWall = leftWall || rightWall;
        currentWallSide = leftWall ? -1 : 1;

        if (ch_isTouchingWall && !c_movementBase.ss_isGrounded && c_rb.linearVelocity.y < 0)
        {

            if (AUTO_LOCK_ON || IsInputTowardsWall())
            {
                StartWallSlide();
            }
            else
            {
                StopWallSlide();
            }
        }
        else
        {
            StopWallSlide();
        }

        if (debugWallInteraction)
        {
            Debug.DrawRay(transform.position, leftWallCheck * m_wallCheckDistance, leftWall ? Color.green : Color.red);
            Debug.DrawRay(transform.position, rightWallCheck * m_wallCheckDistance, rightWall ? Color.green : Color.red);
        }
    }

    bool IsInputTowardsWall()
    {
        return (currentWallSide == -1 && c_movementBase.in_movementInput.x < 0) ||
               (currentWallSide == 1 && c_movementBase.in_movementInput.x > 0);
    }

    void StartWallSlide()
    {
        if (!ch_isSliding && c_movementBase.s_playerState != Platformer2DMovementBase.state.Ledging)
        {
            c_movementBase.s_playerState = Platformer2DMovementBase.state.WallSliding;
            ch_isSliding = true;
        }
    }

    void StopWallSlide()
    {
        if (ch_isSliding && c_movementBase.s_playerState == Platformer2DMovementBase.state.WallSliding)
        {
            c_movementBase.s_playerState = Platformer2DMovementBase.state.Active;
        }
        ch_isSliding = false;
    }
    #endregion

    #region Wall Jumping
    public void HandleWallJump(float jumpInput)
    {
         PerformWallJump();
    }

    public bool CanWallJump()
    {
        if (SLIDING_NECESSARY)
        {
            return ch_isSliding;
        }
        else
        {
            return ch_isTouchingWall;
        }
    }

    void PerformWallJump()
    {
        
        // Determine jump direction (opposite of the current wall side)
        Vector2 jumpDirection = new Vector2(-currentWallSide, m_wallJumpAngle).normalized;

        // Reset vertical velocity for consistent jumps
        c_rb.linearVelocity = Vector2.zero;

        // Apply wall jump force
        c_rb.AddForce(jumpDirection * m_wallJumpForce, ForceMode2D.Impulse);

        // Transition to wall jumping state
        //c_movementBase.s_playerState = Platformer2DMovementBase.state.WallJumping;
    }
    #endregion
}
