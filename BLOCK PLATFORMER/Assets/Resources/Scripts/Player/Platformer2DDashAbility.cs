using System.Collections;
using UnityEngine;

/*
 * Variable Legend:
 * 
 * v_ = value
 * c_ = component
 * ch_ = checker
 * m_ = modifier
 * a_ = activatable modifier
 * chm_ = checker modifier
 * s_ = state (completely defining of movement ex. vaulting)
 * ss_ = substate (helps to find what move but doesn�t define ex. moving or grounded)
 * lm_ = layermask
 * 
 * ao_ = add on (usually script)
 */

public class Platformer2DDashAbility : MonoBehaviour
{
    #region States
    [Header("States + Sub-States")]
    [SerializeField] private bool ss_canDash = true;
    #endregion

    #region Modifiers
    [Header("Dash Settings")]
    [SerializeField] private bool FORCE_BASED;
    [Range(0.0f, 50.0f)][SerializeField] private float m_dashForce = 20f;
    [Range(0.0f, 1.0f)][SerializeField] private float m_dashDuration = 0.2f;
    [Range(0.0f, 5.0f)][SerializeField] private float m_dashCooldown = 1f;

    [Header("Dash QOL")]
    [SerializeField] private bool INTERRUPT_MOVEMENT = true;
    [SerializeField] private bool RESET_AFTER_DASH = true;
    #endregion

    #region Components
    private Rigidbody2D c_rb;
    private Platformer2DMovementBase c_mb;
    private Platformer2DRunning ao_run;
    private Vector2 v_dashDirection;
    #endregion

    #region Initialization
    void ComponentGrab()
    {
        c_rb = GetComponent<Rigidbody2D>();
        c_mb = GetComponent<Platformer2DMovementBase>();
        TryGetComponent<Platformer2DRunning>(out ao_run);
    }

    private void Start()
    {
        ComponentGrab();
    }
    #endregion

    #region Input Functions
    public void DashInput(float value)
    {
        if (value == 1 && c_mb.s_playerState == Platformer2DMovementBase.state.Active && ss_canDash)
        {
            if (FORCE_BASED)
                StartCoroutine(ForceDash());
            else
                StartCoroutine(Dash());
        }
    }
    #endregion

    #region Dash Methods
    IEnumerator Dash()
    {
        // Change state to Dashing
        c_mb.s_playerState = Platformer2DMovementBase.state.Dashing;
        ss_canDash = false;

        // Interrupt movement if enabled
        if (INTERRUPT_MOVEMENT)
        {
            c_rb.linearVelocity = Vector2.zero;
        }

        // Calculate dash direction based on inputing
        v_dashDirection = c_mb.in_movementInput.normalized;

        // Apply dash force
        c_rb.linearVelocity = v_dashDirection * m_dashForce;

        // Temporarily disable base movement
        c_mb.enabled = false;

        // Wait for dash duration
        yield return new WaitForSeconds(m_dashDuration);

        // Reset velocity if enabled
        if (RESET_AFTER_DASH)
        {
            c_rb.linearVelocity = Vector2.zero;
        }

        // Re-enable base movement and reset state
        c_mb.enabled = true;
        c_mb.s_playerState = Platformer2DMovementBase.state.Active;

        // Cooldown before allowing another dash
        yield return new WaitForSeconds(m_dashCooldown);
        ss_canDash = true;
    }
    IEnumerator ForceDash()
    {
        // Change state to Dashing
        c_mb.s_playerState = Platformer2DMovementBase.state.Dashing;
        ss_canDash = false;

        // Interrupt movement if enabled
        if (INTERRUPT_MOVEMENT)
        {
            c_rb.linearVelocity = Vector2.zero;
        }

        // Calculate dash direction based on input
        v_dashDirection = c_mb.in_movementInput.normalized;

        // Apply dash force
        c_rb.AddForce(new Vector2(v_dashDirection.x * m_dashForce, v_dashDirection.y * (m_dashForce / 2)), ForceMode2D.Impulse);

        // Wait for dash duration
        yield return new WaitForSeconds(m_dashDuration);

        // Reset velocity if enabled
        if (RESET_AFTER_DASH)
        {
            c_rb.linearVelocity = Vector2.zero;
        }

        // Re-enable base movement and reset state
        c_mb.s_playerState = Platformer2DMovementBase.state.Active;

        // Cooldown before allowing another dash
        yield return new WaitForSeconds(m_dashCooldown);
        ss_canDash = true;
    }
    #endregion
}
